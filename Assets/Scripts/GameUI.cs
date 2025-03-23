using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private GameManager gameManager;
    public Button startButton;
    public Button endTurnButton;
    public Button restartButton;
    public TextMeshProUGUI logText;
    public ScrollRect logScroll;

    public Image playerBaseSprite;
    public Image playerHeadMask;
    public Image playerTorsoMask;
    public Image enemyBaseSprite;
    public Image enemyHeadMask;
    public Image enemyTorsoMask;

    public TextMeshProUGUI playerEnergyText;
    public TextMeshProUGUI enemyEnergyText;

    public GameObject floatingMessagePrefab; // Префаб текста
    public Transform floatingMessageParent; // Панель или Canvas

    public Sprite headDamage1,
        headDamage2;
    public Sprite torsoDamage1,
        torsoDamage2,
        torsoDamage3;

    private List<string> logHistory = new List<string>();
    private const int maxLogs = 7;

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

            if (logHistory.Count > maxLogs)
            {
                logHistory.RemoveAt(0);
            }

            logText.text = string.Join("\n", logHistory);

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
        Color damageColor = new Color(1, 1, 1, 0);

        if (character.IsPlayer)
        {
            if (bodyPart == "Head")
            {
                targetMask = playerHeadMask;
                if (hits == 1)
                    damageColor = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f);
                else if (hits >= 2)
                    damageColor = new Color(139f / 255f, 0f, 0f, 1f);
            }
            else if (bodyPart == "Torso")
            {
                targetMask = playerTorsoMask;
                if (hits == 1)
                    damageColor = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f);
                else if (hits == 2)
                    damageColor = new Color(178f / 255f, 34f / 255f, 34f / 255f, 1f);
                else if (hits >= 3)
                    damageColor = new Color(139f / 255f, 0f, 0f, 1f);
            }
        }
        else
        {
            if (bodyPart == "Head")
            {
                targetMask = enemyHeadMask;
                if (hits == 1)
                    damageColor = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f);
                else if (hits >= 2)
                    damageColor = new Color(139f / 255f, 0f, 0f, 1f);
            }
            else if (bodyPart == "Torso")
            {
                targetMask = enemyTorsoMask;
                if (hits == 1)
                    damageColor = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f);
                else if (hits == 2)
                    damageColor = new Color(178f / 255f, 34f / 255f, 34f / 255f, 1f);
                else if (hits >= 3)
                    damageColor = new Color(139f / 255f, 0f, 0f, 1f);
            }
        }

        if (targetMask == null)
            return;

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
                child.GetComponent<Image>().color = selectedCards.Contains(cardButton.Card)
                    ? Color.yellow
                    : Color.white;
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

        string logMessage =
            $"<b>Игрок сыграл:</b> {string.Join(", ", playerCards.Select(card => card.Name))}\n"
            + $"<b>Оппонент сыграл:</b> {enemyCard.Name}\n"
            + "<size=18>---------------------------</size>";

        LogAction(logMessage);
    }

    public void ShowFloatingMessage(string message)
    {
        if (floatingMessagePrefab == null || floatingMessageParent == null)
            return;

        GameObject msg = Instantiate(floatingMessagePrefab, floatingMessageParent);
        TextMeshProUGUI text = msg.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = message;

        Destroy(msg, 2f); // удалим через 2 секунды

        // добавим анимацию вверх (если хочешь — через аниматор или LeanTween/DOTween)
        msg.transform.localPosition += new Vector3(0, 30, 0);
    }

    public void ClearLog()
    {
        logHistory.Clear();
        if (logText != null)
        {
            logText.text = "";
        }
    }

    public void UpdateEnergy(Character player, Character enemy)
    {
        if (playerEnergyText != null)
            playerEnergyText.text = $"Энергия: {player.Energy}/30";
        if (enemyEnergyText != null)
            enemyEnergyText.text = $"Энергия: {enemy.Energy}/30";
    }
}
