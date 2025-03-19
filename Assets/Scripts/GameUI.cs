using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    private GameManager gameManager;
    public Button startButton;
    public Button endTurnButton;
    public Button restartButton;
    public TextMeshProUGUI logText;
    public ScrollRect logScroll; // ✅ Добавлен ScrollRect для логов

    // 🎭 Базовые спрайты и повреждения
    public Image playerBaseSprite;
    public Image playerHeadMask;
    public Image playerTorsoMask;
    public Image enemyBaseSprite;
    public Image enemyHeadMask;
    public Image enemyTorsoMask;

    public Sprite headDamage1, headDamage2;
    public Sprite torsoDamage1, torsoDamage2, torsoDamage3;

    private List<string> logHistory = new List<string>(); 
    private const int maxLogs = 7; // ✅ Ограничиваем количество логов

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        startButton.onClick.AddListener(StartGame);
        endTurnButton.onClick.AddListener(EndTurn);
        restartButton.onClick.AddListener(RestartBattle);
        restartButton.gameObject.SetActive(false);

        ResetDamageMasks();
    }

    void StartGame()
    {
        ClearLog();
        LogAction("Бой начался!");
        gameManager.StartBattle();
        startButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);

        ResetDamageMasks();
    }

    void EndTurn()
    {
        gameManager.FinishTurn();
    }

    public void RestartBattle()
    {
        GameManager.Instance.battleEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LogRoundResults(List<string> roundLog)
    {
        foreach (string logEntry in roundLog)
        {
            LogAction(logEntry);
        }
    }

    public void LogAction(string message)
    {
        if (logText != null)
        {
            logHistory.Add(message);

            // ✅ Ограничиваем количество логов
            if (logHistory.Count > maxLogs)
            {
                logHistory.RemoveAt(0);
            }

            logText.text = string.Join("\n", logHistory);

            // ✅ Прокручиваем вниз, чтобы видеть новые логи
            if (logScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                logScroll.verticalNormalizedPosition = 0f;
            }
        }
    }

    public void UpdateCharacterDamage(Character character, string bodyPart, int hits)
{
    Image targetMask = null;
    Color damageColor = new Color(1, 1, 1, 0); // По умолчанию прозрачный (нет урона)

    if (character.IsPlayer)
    {
        if (bodyPart == "Head")
        {
            targetMask = playerHeadMask;
            if (hits == 1) damageColor = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f); // #F08080
            else if (hits >= 2) damageColor = new Color(139f / 255f, 0f, 0f, 1f); // #8B0000
        }
        else if (bodyPart == "Torso")
        {
            targetMask = playerTorsoMask;
            if (hits == 1) damageColor = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f);
            else if (hits == 2) damageColor = new Color(178f / 255f, 34f / 255f, 34f / 255f, 1f); // #B22222
            else if (hits >= 3) damageColor = new Color(139f / 255f, 0f, 0f, 1f); // #8B0000
        }
    }
    else
    {
        if (bodyPart == "Head")
        {
            targetMask = enemyHeadMask;
            if (hits == 1) damageColor = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f);
            else if (hits >= 2) damageColor = new Color(139f / 255f, 0f, 0f, 1f);
        }
        else if (bodyPart == "Torso")
        {
            targetMask = enemyTorsoMask;
            if (hits == 1) damageColor = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f);
            else if (hits == 2) damageColor = new Color(178f / 255f, 34f / 255f, 34f / 255f, 1f);
            else if (hits >= 3) damageColor = new Color(139f / 255f, 0f, 0f, 1f);
        }
    }

    if (targetMask == null) return;

    targetMask.color = damageColor;
}

    private void ResetDamageMasks()
    {
        playerHeadMask.color = new Color(1, 1, 1, 0);
        playerTorsoMask.color = new Color(1, 1, 1, 0);
        enemyHeadMask.color = new Color(1, 1, 1, 0);
        enemyTorsoMask.color = new Color(1, 1, 1, 0);
    }

    public void UpdateCardSelection(List<Card> selectedCards)
    {
        foreach (Transform child in gameManager.playerHand.handPanel)
        {
            CardButton cardButton = child.GetComponent<CardButton>();
            if (cardButton != null && cardButton.Card != null)
            {
                child.GetComponent<Image>().color = selectedCards.Contains(cardButton.Card) ? Color.yellow : Color.white;
            }
        }
    }

    public void RefreshHandUI()
    {
        Debug.Log("[GameUI] Обновление UI руки!");
    }

    public void ShowPlayedCards(List<Card> playerCards, Card enemyCard)
    {
        if (playerCards == null || playerCards.Count == 0 || enemyCard == null)
        {
            Debug.LogError("Ошибка: Одна из карт null!");
            return;
        }

        string logMessage = $"<b>Игрок сыграл:</b> {string.Join(", ", playerCards.Select(card => card.Name))}\n" +
                            $"<b>Оппонент сыграл:</b> {enemyCard.Name}\n" +
                            "<size=18>---------------------------</size>";

        LogAction(logMessage);
    }

    public void ClearLog()
    {
        logHistory.Clear();
        if (logText != null)
        {
            logText.text = "";
        }
    }
}
