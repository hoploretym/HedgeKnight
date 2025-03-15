using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Debug.Log($"[DEBUG] Начало PlayerSelectCard(). Выбрано карт: {playerSelectedCards.Count}");

        if (!waitingForChoices) return;

        if (index < 0 || index >= playerHand.cardsInHand.Count)
        {
            Debug.Log("Некорректный выбор карты!");
            return;
        }

        Card selectedCard = playerHand.cardsInHand[index];

        Debug.Log($"[DEBUG] Карта {selectedCard.Name} сейчас {(playerSelectedCards.Contains(selectedCard) ? "уже выбрана" : "не выбрана")}.");

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

        Debug.Log($"[DEBUG] После выбора: выбрано карт {playerSelectedCards.Count}");

        gameUI.UpdateCardSelection(playerSelectedCards);
    }

    public void FinishTurn()
{
    Debug.Log($"Карт в руке: {playerHand.cardsInHand.Count}, выбранных карт: {playerSelectedCards.Count}");

    if (playerSelectedCards.Count == 0)
    {
        gameUI.LogAction("Выберите хотя бы одну карту перед завершением хода!");
        return;
    }

    waitingForChoices = false;

    // 🏹 Враг выбирает карту
    enemyChosenCard = enemyHand.GetRandomCard();
    gameUI.ClearLog();
    // 🔥 Формируем лог боя с нормальными переносами
    gameUI.LogAction($"Игрок сыграл: <b>{playerSelectedCards[0].Name}</b>");
    gameUI.LogAction($"Оппонент сыграл: <b>{enemyChosenCard.Name}</b>");
    gameUI.LogAction("---------------------------");

    // ⚔️ Применяем карты игрока
    foreach (var card in playerSelectedCards)
    {
        Debug.Log($"Применяем эффект карты {card.Name}.");
        ApplyCardEffects(card, player, enemy, enemyChosenCard);
        playerHand.RemoveCard(card);
    }

    // ⚔️ Применяем карту врага
    if (enemyChosenCard != null)
    {
        ApplyCardEffects(enemyChosenCard, enemy, player, playerSelectedCards.Count > 0 ? playerSelectedCards[0] : null);
        enemyHand.RemoveCard(enemyChosenCard);
    }

    // ❌ Очищаем выбор
    playerSelectedCards.Clear();

    // 🎯 Проверяем победу/поражение
    if (CheckBattleEnd()) return;

    // 🔄 Начинаем новый раунд
    player.ResetEnergy();
    enemy.ResetEnergy();

    playerHand.DrawNewHand();
    enemyHand.DrawNewHand();
    gameUI.RefreshHandUI();

    waitingForChoices = true;
}



    void ApplyCardEffects(Card card, Character attacker, Character target, Card opponentCard)
    {
        if (card == null) return;
        Debug.Log($"{attacker.name} играет {card.Name} против {target.name}");
        card.PlayCard(attacker, target, opponentCard);
    }

    public bool CheckBattleEnd()
    {
        if (player.HeadHits >= 2 || player.TorsoHits >= 3)
        {
            gameUI.LogAction("Игрок проиграл!");
            return true;
        }
        if (enemy.HeadHits >= 2 || enemy.TorsoHits >= 3)
        {
            gameUI.LogAction("Игрок победил!");
            return true;
        }
        return false;
    }

    public void EndBattle(Character loser)
{
    string result = loser.IsPlayer ? "Игрок проиграл!" : "Игрок победил!";
    gameUI.LogAction(result);
    Debug.Log(result);

    // TODO: Здесь можно добавить сцену окончания боя или перезапуск
}
}
