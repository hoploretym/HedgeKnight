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

        // ✅ Сбрасываем защиту до применения карт
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

        playerSelectedCard = null;

        if (CheckBattleEnd())
            return;

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

        gameUI.UpdateEnergy(player, enemy);
        gameUI.RefreshHandUI();
        gameUI.LogRoundResults(roundLog);

        turnNumber++;
        waitingForChoices = true;
    }

    private string ApplyCardEffects(
        Card card,
        Character attacker,
        Character target,
        Card opponentCard
    )
    {
        if (card == null)
            return "Ошибка: карта отсутствует!";

        if (card.Type == CardType.Defense)
        {
            target.SetDefense(card.TargetBodyPart);
            return $"<color=blue>{target.name} активировал защиту на {card.TargetBodyPart}.</color>";
        }
        else
        {
            if (
                opponentCard != null
                && opponentCard.Type == CardType.Defense
                && System.Array.Exists(opponentCard.Counters, c => c == card.Name)
            )
            {
                return $"<color=blue>{target.name} заблокировал атаку {card.Name} с помощью {opponentCard.Name}!</color>";
            }

            target.TakeDamage(card.Damage, card.TargetBodyPart);
            return $"<color=red>{target.name} получил {card.Damage} урона в {card.TargetBodyPart}!</color>";
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
        string result = loser.IsPlayer
            ? "<b><color=red>Игрок проиграл!</color></b>"
            : "<b><color=green>Игрок победил!</color></b>";

        gameUI.ClearLog();
        gameUI.LogAction(result);
        Debug.Log(result);
    }
}
