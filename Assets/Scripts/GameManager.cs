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
        player.ApplyStartOfTurnEffects();
        enemy.ApplyStartOfTurnEffects();

        gameUI.UpdateEnergy(player, enemy);

        playerHand.DrawNewHand();
        enemyHand.DrawNewHand();
    }

    public void PlayerSelectCard(int index)
    {
        if (!waitingForChoices)
            return;
        if (index < 0 || index >= playerHand.cardsInHand.Count)
            return;

        Card selected = playerHand.cardsInHand[index];

        // ❗ Проверка энергии
        if (player.Energy < selected.EnergyCost)
        {
            Debug.Log("Недостаточно энергии для использования этой карты!");
            gameUI.LogAction("<color=red>Недостаточно энергии!</color>");
            return;
        }

        playerSelectedCard = selected;
        gameUI.UpdateCardSelection(new List<Card> { selected });
    }

    private string GetCardHexColor(CardType type)
    {
        return type switch
        {
            CardType.Attack => "#FF0000", // red
            CardType.Defense => "#0000FF", // blue
            CardType.Special => "#FFA500", // orange
            _ => "#FFFFFF", // white (fallback)
        };
    }

    public void FinishTurn()
    {
        if (playerSelectedCard == null)
        {
            gameUI.LogAction("<color=red>Select a card before ending the turn!</color>");
            return;
        }

        waitingForChoices = false;

        player.ResetDefense();
        enemy.ResetDefense();

        enemyChosenCard = enemyHand.GetRandomCard();

        List<string> roundLog = new List<string>();
        roundLog.Add($"<b>Turn {turnNumber}.</b>");

        // Debug
        Debug.Log(
            $"Player plays: {playerSelectedCard.Name} -> {playerSelectedCard.TargetBodyPart}"
        );
        Debug.Log($"Enemy plays: {enemyChosenCard.Name} -> {enemyChosenCard.TargetBodyPart}");

        // Цветовые логи
        string playerColor = GetCardHexColor(playerSelectedCard.Type);
        string enemyColor = GetCardHexColor(enemyChosenCard.Type);

        roundLog.Add(
            $"<b>Card played:</b> <color={playerColor}>{playerSelectedCard.Name} [{playerSelectedCard.TargetBodyPart}]</color>"
        );
        roundLog.Add(
            $"<b>Enemy played:</b> <color={enemyColor}>{enemyChosenCard.Name} [{enemyChosenCard.TargetBodyPart}]</color>"
        );

        // Эффекты карт
        string result = ApplyCardEffects(playerSelectedCard, player, enemy, enemyChosenCard);
        roundLog.Add($"Effect: {result}");

        // Энергия
        // Игрок
        if (playerSelectedCard.Type == CardType.Defense)
        {
            player.GainEnergy(Mathf.Abs(playerSelectedCard.EnergyCost));
        }
        else
        {
            int cost = playerSelectedCard.EnergyCost;
            if (
                (
                    playerSelectedCard.Type == CardType.Attack
                    || playerSelectedCard.Type == CardType.Special
                ) && player.HasDebuff("Legs")
            )
            {
                cost += 1;
                Debug.Log("Slow Movement: Стоимость карты игрока увеличена на 1.");
            }
            player.UseEnergy(cost);
        }

        // Враг
        if (enemyChosenCard.Type == CardType.Defense)
        {
            enemy.GainEnergy(Mathf.Abs(enemyChosenCard.EnergyCost));
        }
        else
        {
            int cost = enemyChosenCard.EnergyCost;
            if (
                (
                    enemyChosenCard.Type == CardType.Attack
                    || enemyChosenCard.Type == CardType.Special
                ) && enemy.HasDebuff("Legs")
            )
            {
                cost += 1;
                Debug.Log("Slow Movement: Стоимость карты врага увеличена на 1.");
            }
            enemy.UseEnergy(cost);
        }

        // Сброс карт
        playerHand.RemoveCard(playerSelectedCard);
        enemyHand.RemoveCard(enemyChosenCard);
        playerSelectedCard = null;

        // Добор (только Debug.Log)
        Card playerDrawn = playerHand.DrawOneCard();
        if (playerDrawn != null)
            Debug.Log($"Player draws: {playerDrawn.Name}");

        Card enemyDrawn = enemyHand.DrawOneCard();
        if (enemyDrawn != null)
            Debug.Log($"Enemy draws: {enemyDrawn.Name}");

        // Обновления
        playerHand.UpdateHandUI();
        gameUI.UpdateEnergy(player, enemy);
        gameUI.RefreshHandUI();
        gameUI.LogRoundResults(roundLog);
        gameUI.UpdateAllHPText();

        if (player.IsDead() && enemy.IsDead())
        {
            EndBattle(player);
            return;
        }

        if (pendingLoser != null)
        {
            EndBattle(pendingLoser);
            return;
        }

        if (CheckBattleEnd())
            return;
        if (player.HasDebuff("Head"))
            player.IncrementBrokenHelmetCounter();

        turnNumber++;
        player.ApplyStartOfTurnEffects();
        enemy.ApplyStartOfTurnEffects();
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
        if (
            (card.Type == CardType.Attack || card.Type == CardType.Special)
            && attacker.HasDebuff("Arms")
        )
        {
            finalDamage = Mathf.Max(0, finalDamage - 1);
            Debug.Log($"{attacker.Name} имеет дебафф Unsteady Grip — урон снижен до {finalDamage}");
        }
        if (sameTarget)
            finalDamage *= 2;

        string log = $"{card.Name} -> {card.TargetBodyPart}";
        if (sameTarget)
            log += " (x2)";

        switch (card.Type)
        {
            case CardType.Attack:
                switch (result)
                {
                    case Outcome.Win:
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);
                        log += " – Hit!";
                        break;

                    case Outcome.Lose:
                        log += " – Blocked.";
                        break;

                    case Outcome.Tie:
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);

                        int retaliationDmg = enemyCard.Damage;
                        if (enemyCard.TargetBodyPart == card.TargetBodyPart)
                            retaliationDmg *= 2;

                        attacker.TakeDamage(retaliationDmg, enemyCard.TargetBodyPart);
                        log +=
                            $" – Tie. {defender.Name} takes {finalDamage}, {attacker.Name} takes {retaliationDmg}";
                        break;
                }
                break;

            case CardType.Defense:
                switch (result)
                {
                    case Outcome.Win:
                        log += " – Energy restored.";
                        break;

                    case Outcome.Lose:
                        int incomingDamage = sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage;
                        attacker.TakeDamage(incomingDamage, enemyCard.TargetBodyPart);
                        log += $" – Block failed. Took {incomingDamage}.";
                        break;

                    case Outcome.Tie:
                        log += " – Partial block.";
                        break;
                }
                break;

            case CardType.Special:
                switch (result)
                {
                    case Outcome.Win:
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);
                        log += " – Special success!";
                        break;

                    case Outcome.Lose:
                        int dmg = sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage;
                        attacker.TakeDamage(dmg, enemyCard.TargetBodyPart);
                        log += $" – Countered. Took {dmg}.";
                        break;

                    case Outcome.Tie:
                        defender.TakeDamage(finalDamage, card.TargetBodyPart);

                        int counterDmg = sameTarget ? enemyCard.Damage * 2 : enemyCard.Damage;
                        attacker.TakeDamage(counterDmg, enemyCard.TargetBodyPart);
                        log +=
                            $" – Tie. {defender.Name} takes {finalDamage}, {attacker.Name} takes {counterDmg}";
                        break;
                }
                break;
        }

        return log;
    }

    public void RegisterPendingDeath(Character c)
    {
        if (!battleEnded && pendingLoser == null)
        {
            pendingLoser = c;
        }
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

        bool playerLost = loser.IsPlayer;
        string resultMessage = playerLost ? "Игрок проиграл!" : "Игрок победил!";

        Debug.Log(resultMessage);
        gameUI.ShowGameResult(!playerLost);
    }
}
