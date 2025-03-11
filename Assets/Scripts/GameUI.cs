using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    private GameManager gameManager;
    public Button startButton;
    public Button endTurnButton;
    public TextMeshProUGUI logText;
    public Image playerHead;
    public Image playerTorso;
    public Image enemyHead;
    public Image enemyTorso;
    public Sprite[] damageSprites;

    void Start()
    {
        // Если GameManager не задан через Inspector, находим его
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager не найден! Проверь, добавлен ли он в сцену.");
            return;
        }

        startButton.onClick.AddListener(StartGame);
        endTurnButton.onClick.AddListener(EndTurn);
        endTurnButton.gameObject.SetActive(false);
    }

    void StartGame()
    {
        if (gameManager == null) return;

        ClearLog();
        LogAction("Бой начался!");

        gameManager.StartBattle();
        startButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(true);
    }

    void EndTurn()
    {
        if (gameManager == null) return;

        gameManager.FinishTurn();
    }

    public void UpdateCharacterDamage(Character character, string bodyPart, int hits)
    {
        Image targetImage = null;

        if (character.IsPlayer)
        {
            if (bodyPart == "Head") targetImage = playerHead;
            else if (bodyPart == "Torso") targetImage = playerTorso;
        }
        else
        {
            if (bodyPart == "Head") targetImage = enemyHead;
            else if (bodyPart == "Torso") targetImage = enemyTorso;
        }

        if (targetImage == null)
        {
            Debug.LogError($"Не найдено изображение для {bodyPart} у {character.name}!");
            return;
        }

        if (hits == 1) targetImage.sprite = damageSprites[1];
        else if (hits >= 2) targetImage.sprite = damageSprites[2];
    }

    public void ShowPlayedCards(Card playerCard, Card enemyCard)
    {
        if (playerCard == null || enemyCard == null)
        {
            Debug.LogError("Ошибка: Одна из карт null!");
            return;
        }
        LogAction($"Игрок сыграл: {playerCard.Name}, Враг сыграл: {enemyCard.Name}");
    }

    public void LogAction(string message)
    {
        if (logText != null)
        {
            logText.text += message + "\n";
        }
    }

    public void ClearLog()
    {
        if (logText != null)
        {
            logText.text = "";
        }
    }

    public void RefreshHandUI()
    {
        LogAction("Обновление руки!");
    }
}
