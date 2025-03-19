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
    private List<Card> playerSelectedCards = new List<Card>();
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
        gameUI.ClearLog(); // Очистка перед началом боя
        Deck playerDeck = GameObject.Find("DeckManager")?.GetComponent<Deck>();
        Deck enemyDeck = GameObject.Find("DeckManagerEnemy")?.GetComponent<Deck>();

        if (playerDeck == null || enemyDeck == null)
        {
            Debug.LogError("Ошибка: Не найдены DeckManager или DeckManagerEnemy!");
            return;
        }

        Debug.Log($"Назначаем {playerHand?.gameObject.name ?? "NULL"} колоду: {playerDeck.gameObject.name}");
        Debug.Log($"Назначаем {enemyHand?.gameObject.name ?? "NULL"} колоду: {enemyDeck.gameObject.name}");

        if (playerHand == null)
        {
            Debug.LogError("Ошибка: playerHand == null! Проверь, добавлен ли PlayerHandManager в сцену и назначен ли в GameManager.");
            return;
        }

        if (enemyHand == null)
        {
            Debug.LogError("Ошибка: enemyHand == null! Проверь, добавлен ли EnemyHandManager в сцену и назначен ли в GameManager.");
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

        if (index < 0 || index >= playerHand.cardsInHand.Count)
        {
            Debug.Log("Некорректный выбор карты!");
            return;
        }

        Card selectedCard = playerHand.cardsInHand[index];

        if (playerSelectedCards.Contains(selectedCard))
        {
            playerSelectedCards.Remove(selectedCard);
            Debug.Log($"Карта {selectedCard.Name} убрана из выбранных.");
        }
        else if (player.Energy >= selectedCard.EnergyCost)
        {
            playerSelectedCards.Add(selectedCard);
            Debug.Log($"Карта {selectedCard.Name} добавлена в выбранные.");
        }
        else
        {
            Debug.Log("Недостаточно энергии!");
        }

        gameUI.UpdateCardSelection(playerSelectedCards);
    }

    public void FinishTurn()
{
    if (playerSelectedCards.Count == 0)
    {
        gameUI.LogAction("<color=red>Выберите хотя бы одну карту перед завершением хода!</color>");
        return;
    }

    waitingForChoices = false;
    enemyChosenCard = enemyHand.GetRandomCard();
    gameUI.ClearLog();

    List<string> roundLog = new List<string>();
    roundLog.Add($"<b>Ход {turnNumber}.</b>");

    // 🔥 Игрок разыгрывает карты
    foreach (var card in playerSelectedCards)
    {
        string result = ApplyCardEffects(card, player, enemy, enemyChosenCard);
        roundLog.Add($"<b>Сыграна карта:</b> {card.Name}");
        roundLog.Add($"Эффект: {result}");
        playerHand.RemoveCard(card);
    }

    // 🔥 Противник разыгрывает карту
    if (enemyChosenCard != null)
    {
        string enemyResult = ApplyCardEffects(enemyChosenCard, enemy, player, playerSelectedCards.Count > 0 ? playerSelectedCards[0] : null);
        roundLog.Add($"<b>Противник сыграл карту:</b> {enemyChosenCard.Name}");
        roundLog.Add($"Эффект: {enemyResult}");
        enemyHand.RemoveCard(enemyChosenCard);
    }

    playerSelectedCards.Clear();

    if (CheckBattleEnd()) return;

    // 🔄 Переход к следующему ходу
    player.ResetEnergy();
    enemy.ResetEnergy();
    playerHand.DrawNewHand();
    enemyHand.DrawNewHand();
    gameUI.RefreshHandUI();

    gameUI.LogRoundResults(roundLog);
    turnNumber++; // Увеличиваем счетчик хода
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
    if (battleEnded) return true; // Если бой уже завершился, просто выходим

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
    if (battleEnded) return; // Если бой уже завершен, просто выходим

    battleEnded = true; // Фиксируем, что бой завершен
    string result = loser.IsPlayer ? "<b><color=red>Игрок проиграл!</color></b>" : "<b><color=green>Игрок победил!</color></b>";

    gameUI.ClearLog(); // Очищаем предыдущие сообщения, чтобы оставить только финальный результат
    gameUI.LogAction(result);
    Debug.Log(result);
}
}
