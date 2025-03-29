using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public CharacterManager playerController;
    public CharacterManager enemyController;
    public HandManager playerHand;
    public EnemyHandManager enemyHand;
    public GameUI gameUI;

    private Character player;
    private Character enemy;
    private Card playerSelectedCard;
    private Card enemyChosenCard;
    private bool waitingForChoices = true;
    public bool battleEnded = false;
    private int turnNumber = 1;
    private Character pendingLoser = null;

    private bool playerDead = false;
    private bool enemyDead = false;

    public enum Outcome
    {
        Win,
        Lose,
        Tie,
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        player = playerController.GetCharacter();
        enemy = enemyController.GetCharacter();
        player.IsPlayer = true;
        enemy.IsPlayer = false;
    }

    public void StartBattle()
    {
        Debug.Log("Бой начался!");
        gameUI.ClearLog();
        Deck playerDeck = GameObject.Find("DeckManager")?.GetComponent<Deck>();
        Deck enemyDeck = GameObject.Find("DeckManagerEnemy")?.GetComponent<Deck>();

        if (playerDeck == null || enemyDeck == null)
        {
            Debug.LogError("Ошибка: Не найдены DeckManager или DeckManagerEnemy!");
            return;
        }

        if (playerHand == null || enemyHand == null)
        {
            Debug.LogError("Ошибка: Не найдены HandManager у игрока или противника!");
            return;
        }

        playerHand.Initialize(playerDeck);
        enemyHand.Initialize(enemyDeck);

        player.ResetEnergy();
        enemy.ResetEnergy();

        gameUI.UpdateEnergy(player, enemy);

        playerHand.DrawNewHand();
        enemyHand.DrawNewHand();
    }

    public void PlayerSelectCard(int index)
    {
        if (!waitingForChoices)
            return;

        if (index < 0 || index >= playerHand.cardsInHand.Count)
        {
            Debug.LogError(
                $"[PlayerSelectCard] Ошибка: Некорректный индекс карты ({index})! Карт в руке: {playerHand.cardsInHand.Count}"
            );
            return;
        }

        Card selectedCard = playerHand.cardsInHand[index];

        if (playerSelectedCard == selectedCard)
        {
            playerSelectedCard = null;
            gameUI.UpdateCardSelection(new List<Card>());
            Debug.Log($"[GameManager] Снято выделение с карты: {selectedCard.Name}");
        }
        else
        {
            playerSelectedCard = selectedCard;
            gameUI.UpdateCardSelection(new List<Card> { playerSelectedCard });
            Debug.Log($"[GameManager] Выбрана карта: {selectedCard.Name}");
        }
    }

    public void FinishTurn()
    {
        if (playerSelectedCard == null)
        {
            gameUI.LogAction("<color=red>Выберите карту перед завершением хода!</color>");
            return;
        }

        waitingForChoices = false;

        // ✅ Сбрасываем защиту ДО применения карт
        player.ResetDefense();
        enemy.ResetDefense();

        enemyChosenCard = enemyHand.GetRandomCard();

        List<string> roundLog = new List<string>();
        roundLog.Add($"<b>Ход {turnNumber}.</b>");

        // 💥 Применяем карту игрока
        string result = ApplyCardEffects(playerSelectedCard, player, enemy, enemyChosenCard);
        roundLog.Add($"<b>Сыграна карта:</b> {playerSelectedCard.Name}");
        roundLog.Add($"Эффект: {result}");
        playerHand.RemoveCard(playerSelectedCard);

        if (playerSelectedCard.Type == CardType.Defense)
            player.GainEnergy(Mathf.Abs(playerSelectedCard.EnergyCost));
        else
            player.UseEnergy(playerSelectedCard.EnergyCost);

        // 💥 Применяем карту врага
        if (enemyChosenCard != null)
        {
            string enemyResult = ApplyCardEffects(
                enemyChosenCard,
                enemy,
                player,
                playerSelectedCard
            );
            roundLog.Add($"<b>Противник сыграл карту:</b> {enemyChosenCard.Name}");
            roundLog.Add($"Эффект: {enemyResult}");
            enemyHand.RemoveCard(enemyChosenCard);

            if (enemyChosenCard.Type == CardType.Defense)
                enemy.GainEnergy(Mathf.Abs(enemyChosenCard.EnergyCost));
            else
                enemy.UseEnergy(enemyChosenCard.EnergyCost);
        }

        // Сброс выбранной карты
        playerSelectedCard = null;

        // 🔁 Добор карт
        int playerHandBefore = playerHand.cardsInHand.Count;
        int enemyHandBefore = enemyHand.CardsInHandCount;

        if (playerHandBefore > 0)
        {
            Card newCard = playerHand.DrawOneCard();
            if (newCard != null)
                roundLog.Add($"<color=yellow>Игрок добирает карту: {newCard.Name}</color>");
        }

        if (enemyHandBefore > 0)
        {
            Card newCard = enemyHand.DrawOneCard();
            if (newCard != null)
                roundLog.Add("<color=yellow>Противник добирает карту</color>");
        }

        // ✅ Только теперь — обновляем UI
        gameUI.UpdateEnergy(player, enemy);
        gameUI.RefreshHandUI();
        gameUI.LogRoundResults(roundLog);
        gameUI.UpdateAllHPText();

        // ✅ Завершение боя после отображения логов
        if (pendingLoser != null)
        {
            EndBattle(pendingLoser);
            return;
        }

        if (playerDead && enemyDead)
        {
            EndBattle(player); // Игрок проигрывает при ничьей
            return;
        }

        if (playerDead)
        {
            EndBattle(player);
            return;
        }
        if (enemyDead)
        {
            EndBattle(enemy);
            return;
        }

        if (CheckBattleEnd())
            return;

        turnNumber++;
        waitingForChoices = true;
    }

    private Outcome GetOutcome(CardType attacker, CardType defender)
    {
        if (attacker == defender)
            return Outcome.Tie;

        if (
            (attacker == CardType.Attack && defender == CardType.Special)
            || (attacker == CardType.Special && defender == CardType.Defense)
            || (attacker == CardType.Defense && defender == CardType.Attack)
        )
        {
            return Outcome.Win;
        }

        return Outcome.Lose;
    }

    public string ApplyCardEffects(
        Card card,
        Character attacker,
        Character defender,
        Card enemyCard
    )
    {
        Outcome result = GetOutcome(card.Type, enemyCard.Type);
        bool sameTarget = card.TargetBodyPart == enemyCard.TargetBodyPart;

        int finalDamage = card.Damage;
        if (sameTarget)
            finalDamage *= 2;

        string log = "";

        switch (card.Type)
        {
            case CardType.Attack:
                switch (result)
                {
                    case Outcome.Win:
                        log += "Победа: удар проходит!";
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);
                        break;

                    case Outcome.Lose:
                        log += "Проигрыш: атака заблокирована.";
                        if (sameTarget)
                        {
                            int energyGain = Mathf.Abs(enemyCard.EnergyCost) * 2;
                            defender.GainEnergy(energyGain);
                            log += $" Противник восстанавливает {energyGain} энергии!";
                        }
                        break;

                    case Outcome.Tie:
                        log += "Ничья: обмен ударами!";
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);
                        attacker.TakeDamage(
                            sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage,
                            enemyCard.TargetBodyPart
                        );
                        break;
                }
                break;

            case CardType.Defense:
                int energyToGain = Mathf.Abs(card.EnergyCost);
                switch (result)
                {
                    case Outcome.Win:
                        if (sameTarget)
                            energyToGain *= 2;
                        attacker.GainEnergy(energyToGain);
                        log += $"Победа: восстановлено {energyToGain} энергии.";
                        break;

                    case Outcome.Lose:
                        attacker.GainEnergy(energyToGain);
                        int incomingDamage = sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage;
                        attacker.TakeDamage(incomingDamage, enemyCard.TargetBodyPart);
                        log +=
                            $"Проигрыш: получен урон {incomingDamage}, восстановлено {energyToGain} энергии.";
                        break;

                    case Outcome.Tie:
                        attacker.GainEnergy(energyToGain);
                        log += $"Ничья: восстановлено {energyToGain} энергии.";
                        break;
                }
                break;

            case CardType.Special:
                switch (result)
                {
                    case Outcome.Win:
                        log += "Победа: специальный приём сработал!";
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);
                        break;

                    case Outcome.Lose:
                        int dmg = sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage;
                        attacker.TakeDamage(dmg, enemyCard.TargetBodyPart);
                        log += $"Проигрыш: получен урон {dmg}.";
                        break;

                    case Outcome.Tie:
                        log += "Ничья: обмен ударами!";
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);
                        attacker.TakeDamage(
                            sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage,
                            enemyCard.TargetBodyPart
                        );
                        break;
                }
                break;
        }

        return log;
    }

    public void RegisterPendingDeath(Character c)
    {
        if (battleEnded)
            return;

        if (c.IsPlayer)
            playerDead = true;
        else
            enemyDead = true;
    }

    public bool CheckBattleEnd()
    {
        if (battleEnded)
            return true;

        if (player.HeadHP <= 0 || player.TorsoHP <= 0)
        {
            EndBattle(player);
            return true;
        }
        if (enemy.HeadHP <= 0 || enemy.TorsoHP <= 0)
        {
            EndBattle(enemy);
            return true;
        }
        return false;
    }

    public void EndBattle(Character loser)
    {
        if (battleEnded)
            return;

        battleEnded = true;
        playerDead = false;
        enemyDead = false;

        bool playerLost = loser.IsPlayer;
        string resultMessage = playerLost ? "Игрок проиграл!" : "Игрок победил!";

        Debug.Log(resultMessage);
        gameUI.ShowGameResult(!playerLost);
    }
}
