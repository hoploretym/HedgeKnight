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
    private Card playerSelectedCard; // Теперь только 1 карта
    private Card enemyChosenCard;
    private bool waitingForChoices = true;
    public bool battleEnded = false;
    private int turnNumber = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        playerHand.DrawNewHand();
        enemyHand.DrawNewHand();
    }

public void PlayerSelectCard(int index)
{
    if (!waitingForChoices) return;

    // ✅ Исправляем проверку, чтобы индекс не вызывал крэш
    if (index < 0 || index >= playerHand.cardsInHand.Count)
    {
        Debug.LogError($"[PlayerSelectCard] Ошибка: Некорректный индекс карты ({index})! Карт в руке: {playerHand.cardsInHand.Count}");
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
    enemyChosenCard = enemyHand.GetRandomCard();

    List<string> roundLog = new List<string>();
    roundLog.Add($"<b>Ход {turnNumber}.</b>");

    // ✅ Игрок разыгрывает карту
    string result = ApplyCardEffects(playerSelectedCard, player, enemy, enemyChosenCard);
    roundLog.Add($"<b>Сыграна карта:</b> {playerSelectedCard.Name}");
    roundLog.Add($"Эффект: {result}");
    playerHand.RemoveCard(playerSelectedCard);

    // ✅ Противник разыгрывает карту
    if (enemyChosenCard != null)
    {
        string enemyResult = ApplyCardEffects(enemyChosenCard, enemy, player, playerSelectedCard);
        roundLog.Add($"<b>Противник сыграл карту:</b> {enemyChosenCard.Name}");
        roundLog.Add($"Эффект: {enemyResult}");
        enemyHand.RemoveCard(enemyChosenCard);
    }

    playerSelectedCard = null; 

    if (CheckBattleEnd()) return;

    // ✅ Запоминаем количество карт перед обновлением
    int playerHandBefore = playerHand.cardsInHand.Count;
    int enemyHandBefore = enemyHand.CardsInHandCount;

    bool playerHandRefilled = playerHandBefore == 0;
    bool enemyHandRefilled = enemyHandBefore == 0;

if (playerHandRefilled)
{
    playerHand.DrawNewHand();
    roundLog.Add("<color=yellow><b>Обновление руки!</b></color>"); // ✅ Лог только при обновлении
}

if (enemyHandRefilled)
{
    enemyHand.DrawNewHand();
}

    gameUI.RefreshHandUI();
    gameUI.LogRoundResults(roundLog);

    turnNumber++; 
    waitingForChoices = true;
}



    private string ApplyCardEffects(Card card, Character attacker, Character target, Card opponentCard)
    {
        if (card == null) return "Ошибка: карта отсутствует!";

        if (card.Type == CardType.Defense)
        {
            target.SetDefense(card.TargetBodyPart);
            return $"<color=blue>{target.name} активировал защиту на {card.TargetBodyPart}.</color>";
        }
        else
        {
            if (opponentCard != null && opponentCard.Type == CardType.Defense &&
                System.Array.Exists(opponentCard.Counters, c => c == card.Name))
            {
                return $"<color=blue>{target.name} заблокировал атаку {card.Name} с помощью {opponentCard.Name}!</color>";
            }

            target.TakeDamage(card.Damage, card.TargetBodyPart);
            return $"<color=red>{target.name} получил {card.Damage} урона в {card.TargetBodyPart}!</color>";
        }
    }

    public bool CheckBattleEnd()
    {
        if (battleEnded) return true; 

        if (player.HeadHits >= 2 || player.TorsoHits >= 3)
        {
            EndBattle(player);
            return true;
        }
        if (enemy.HeadHits >= 2 || enemy.TorsoHits >= 3)
        {
            EndBattle(enemy);
            return true;
        }

        return false;
    }

    public void EndBattle(Character loser)
    {
        if (battleEnded) return; 

        battleEnded = true;
        string result = loser.IsPlayer ? "<b><color=red>Игрок проиграл!</color></b>" : "<b><color=green>Игрок победил!</color></b>";

        gameUI.ClearLog();
        gameUI.LogAction(result);
        Debug.Log(result);
    }
}
