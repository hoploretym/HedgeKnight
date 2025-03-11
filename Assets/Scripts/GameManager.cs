using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CharacterManager playerController;
    public CharacterManager enemyController;
public HandManager playerHand;
public HandManager enemyHand;
    public GameUI gameUI;
    
    private Character player;
    private Character enemy;
    private bool waitingForChoices = true;
    private Card playerChosenCard = null;
    private Card enemyChosenCard = null;

    void Start()
    {
        if (playerController == null || enemyController == null)
        {
            Debug.LogError("GameManager: Не установлены ссылки на CharacterManager!");
            return;
        }

        player = playerController.GetCharacter();
        enemy = enemyController.GetCharacter();
        player.IsPlayer = true;
        enemy.IsPlayer = false;
    }

public void StartBattle()
{
    Debug.Log("Инициализация StartBattle()");
    Debug.Log($"PlayerHand: {playerHand.gameObject.name}, EnemyHand: {enemyHand.gameObject.name}");

    
    // Передаем колоду в HandManager
    playerHand.Initialize(FindObjectOfType<Deck>());
    enemyHand.Initialize(FindObjectOfType<Deck>());

    player.ResetEnergy();
    enemy.ResetEnergy();
    
 playerHand.DrawNewHand();
enemyHand.DrawNewHand();
    StartCoroutine(GameLoop());
}


    IEnumerator GameLoop()
    {
        while (true)
        {
            waitingForChoices = true;
            playerChosenCard = null;
            enemyChosenCard = enemyHand.GetRandomCard(); // Теперь этот метод есть

            gameUI.LogAction("Select a card");
            yield return new WaitUntil(() => playerChosenCard != null);

            gameUI.ShowPlayedCards(playerChosenCard, enemyChosenCard);
            ApplyCardEffects(playerChosenCard, enemyChosenCard);
            
            if (CheckBattleEnd()) yield break;

            player.ResetEnergy();
            enemy.ResetEnergy();
            playerHand.DrawNewHand();
            enemyHand.DrawNewHand();
            gameUI.RefreshHandUI();
        }
    }

public void PlayerSelectCard(int index)
{
    if (!waitingForChoices || index == -1) return;

    playerChosenCard = playerHand.cardsInHand[index]; // Запоминаем карту
    playerHand.RemoveCard(index); // Убираем карту из руки
    gameUI.LogAction($"Игрок выбрал карту: {playerChosenCard.Name}");
}

    

    public void FinishTurn()
    {
        if (playerChosenCard == null)
        {
            gameUI.LogAction("Select a card before finishing!");
            return;
        }

        waitingForChoices = false;
    }

    void ApplyCardEffects(Card playerCard, Card enemyCard)
    {
        playerCard.PlayCard(player, enemy);
        enemyCard.PlayCard(enemy, player);
        gameUI.LogAction($"Игрок сыграл {playerCard.Name}, враг сыграл {enemyCard.Name}!");
    }

    bool CheckBattleEnd()
    {
        if (player.HeadHits >= 2 || player.TorsoHits >= 3)
        {
            gameUI.LogAction("Игрок проиграл!");
            StopAllCoroutines();
            return true;
        }
        if (enemy.HeadHits >= 2 || enemy.TorsoHits >= 3)
        {
            gameUI.LogAction("Игрок победил!");
            StopAllCoroutines();
            return true;
        }
        return false;
    }
}
