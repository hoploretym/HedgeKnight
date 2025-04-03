using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private GameManager gameManager; // Сущность, которая управляет ходами и боем
    public Button startButton; // Кнопка начала боя
    public Button endTurnButton; // Кнопка завершения хода
    public Button restartButton; // Кнопка рестарта боя
    public TextMeshProUGUI logText; // Логирование боя
    public ScrollRect logScroll; // Ответственен за скролл логов боя

    // Спрайты и маски отображения частей тела Player
    public Image playerBaseSprite; // Базовый спрайт рыцаря Player
    public Image playerHeadMask; // Прозрачная окрашиваемая маска, которая лежит на голове
    public Image playerTorsoMask; // Прозрачная окрашиваемая маска, которая лежит на торса
    public Image playerArmsMask; // Прозрачная окрашиваемая маска, которая лежит на руках
    public Image playerLegsMask; // Прозрачная окрашиваемая маска, которая лежит на ногах

    // Спрайты и маски отображения частей тела Enemy
    public Image enemyBaseSprite; // Базовый спрайт рыцаря Player
    public Image enemyHeadMask; // Прозрачная окрашиваемая маска, которая лежит на голове
    public Image enemyTorsoMask; // Прозрачная окрашиваемая маска, которая лежит на торса
    public Image enemyArmsMask; // Прозрачная окрашиваемая маска, которая лежит на руках
    public Image enemyLegsMask; // Прозрачная окрашиваемая маска, которая лежит на ногах

    // Текстовое отображение ХП частей тела
    public TextMeshProUGUI playerHeadHPText;
    public TextMeshProUGUI playerTorsoHPText;
    public TextMeshProUGUI playerArmsHPText;
    public TextMeshProUGUI playerLegsHPText;

    public TextMeshProUGUI enemyHeadHPText;
    public TextMeshProUGUI enemyTorsoHPText;
    public TextMeshProUGUI enemyArmsHPText;
    public TextMeshProUGUI enemyLegsHPText;

    // Переменные цветов для отображения урона
    private readonly Color lightRed = new Color(240f / 255f, 128f / 255f, 128f / 255f, 1f); // Светло-красный цвет после получения 1ед. урона
    private readonly Color darkRed = new Color(139f / 255f, 0f, 0f, 1f); // Темно-красный цвет после получения максимального урона по ХП

    // Тултипы отображения ХП при наведении курсора на маску

    // Отображение энергии Player и Enemy
    public TextMeshProUGUI playerEnergyText;
    public TextMeshProUGUI enemyEnergyText;

    // Сообщение о результатах боя
    public CanvasGroup gameResultPanel;
    public TextMeshProUGUI gameResultText;

    public Slider playerEnergyBar;
    public Slider enemyEnergyBar;

    private List<string> logHistory = new List<string>();
    private const int maxLogs = 4;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        startButton.onClick.AddListener(StartGame);
        endTurnButton.onClick.AddListener(EndTurn);
        restartButton.onClick.AddListener(RestartBattle);
        restartButton.gameObject.SetActive(false);
        DebugAllBodyParts();
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
        Debug.Log($"[LogAction] {message}");

        logHistory.Add(message);

        // ⚠️ НЕ обрезаем до maxLogs!
        // Иначе ScrollView не будет знать, что есть лишние строки

        logText.text = string.Join("\n", logHistory);

        // ⬇️ Автоскролл вниз (опционально — можешь убрать)
        if (logScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            logScroll.verticalNormalizedPosition = 0f;
        }
    }

    public void UpdateCharacterDamage(Character character, string bodyPart, int currentHP)
    {
        Image targetMask = null;
        int maxHP = character.GetMaxHP(bodyPart);

        if (character.IsPlayer)
        {
            switch (bodyPart)
            {
                case "Head":
                    targetMask = playerHeadMask;
                    break;
                case "Torso":
                    targetMask = playerTorsoMask;
                    break;
                case "Arms":
                    targetMask = playerArmsMask;
                    break;
                case "Legs":
                    targetMask = playerLegsMask;
                    break;
            }
        }
        else
        {
            switch (bodyPart)
            {
                case "Head":
                    targetMask = enemyHeadMask;
                    break;
                case "Torso":
                    targetMask = enemyTorsoMask;
                    break;
                case "Arms":
                    targetMask = enemyArmsMask;
                    break;
                case "Legs":
                    targetMask = enemyLegsMask;
                    break;
            }
        }

        UpdateDamageMask(targetMask, currentHP, maxHP);
    }

    private IEnumerator FlashHitMask(Image mask, Color targetColor)
    {
        if (mask == null)
            yield break;

        float duration = 0.7f;
        float elapsed = 0f;

        mask.color = Color.white;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Color lerped = Color.Lerp(Color.white, targetColor, t);
            mask.color = lerped;
            elapsed += Time.deltaTime;
            yield return null;
        }

        mask.color = targetColor;
    }

    public void UpdateAllHPText()
    {
        if (GameManager.Instance == null)
            return;

        Character player = GameManager.Instance.playerController.GetCharacter();
        Character enemy = GameManager.Instance.enemyController.GetCharacter();

        UpdateSingleHPText(player, playerHeadHPText, "Head");
        UpdateSingleHPText(player, playerTorsoHPText, "Torso");
        UpdateSingleHPText(player, playerArmsHPText, "Arms");
        UpdateSingleHPText(player, playerLegsHPText, "Legs");

        UpdateSingleHPText(enemy, enemyHeadHPText, "Head");
        UpdateSingleHPText(enemy, enemyTorsoHPText, "Torso");
        UpdateSingleHPText(enemy, enemyArmsHPText, "Arms");
        UpdateSingleHPText(enemy, enemyLegsHPText, "Legs");
    }

    private void UpdateSingleHPText(Character c, TextMeshProUGUI text, string part)
    {
        if (text == null || c == null)
            return;

        int current = c.GetCurrentHP(part);
        int max = c.GetMaxHP(part);
        text.text = $"{current}/{max}";
    }

    private void UpdateDamageMask(Image mask, int currentHP, int maxHP)
    {
        if (mask == null)
            return;

        if (currentHP == maxHP)
        {
            mask.color = new Color(1, 1, 1, 0); // прозрачный
        }
        else
        {
            float damagePercent = 1f - ((float)currentHP / maxHP);
            Color newColor = Color.Lerp(lightRed, darkRed, damagePercent);
            Debug.Log(
                $"[UpdateDamageMask] {mask.name}: HP {currentHP}/{maxHP} → {damagePercent * 100:F0}% урона → цвет {newColor}"
            );
            StartCoroutine(FlashHitMask(mask, newColor));
        }
    }

    private void ResetDamageMasks()
    {
        playerHeadMask.color = new Color(1, 1, 1, 0);
        playerTorsoMask.color = new Color(1, 1, 1, 0);
        playerArmsMask.color = new Color(1, 1, 1, 0);
        playerLegsMask.color = new Color(1, 1, 1, 0);

        enemyHeadMask.color = new Color(1, 1, 1, 0);
        enemyTorsoMask.color = new Color(1, 1, 1, 0);
        enemyArmsMask.color = new Color(1, 1, 1, 0);
        enemyLegsMask.color = new Color(1, 1, 1, 0);
    }

    public void DebugAllBodyParts()
    {
        var handlers = FindObjectsOfType<BodyPartTooltipHandler>();
        Debug.Log($"🧠 Найдено {handlers.Length} BodyPartTooltipHandler'ов");

        foreach (var handler in handlers)
        {
            Debug.Log(
                $"➡️ На объекте [{handler.gameObject.name}] — bodyPart = {handler.bodyPart}, isPlayer = {handler.isPlayer}"
            );
        }
    }

    public void UpdateCardSelection(List<Card> selectedCards)
    {
        foreach (Transform child in gameManager.playerHand.handPanel)
        {
            CardButton cardButton = child.GetComponent<CardButton>();
            if (cardButton != null && cardButton.Card != null)
            {
                bool isSelected = selectedCards.Contains(cardButton.Card);
                cardButton.SetOutline(isSelected);
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
            playerEnergyText.text = $"Energy: {player.Energy}/30";
        if (enemyEnergyText != null)
            enemyEnergyText.text = $"Energy: {enemy.Energy}/30";

        if (playerEnergyBar != null)
            playerEnergyBar.value = player.Energy;
        if (enemyEnergyBar != null)
            enemyEnergyBar.value = enemy.Energy;
    }

    public void ShowGameResult(bool isPlayerWin)
    {
        StartCoroutine(FadeGameResult(isPlayerWin));
    }

    private IEnumerator FadeGameResult(bool isPlayerWin)
    {
        gameResultPanel.alpha = 0f;
        gameResultPanel.gameObject.SetActive(true);

        gameResultText.text = isPlayerWin ? "ПОБЕДА!" : "ПОРАЖЕНИЕ";
        gameResultText.color = isPlayerWin ? Color.green : Color.red;

        // ⏱ подождать, чтобы сначала увидеть логи
        yield return new WaitForSeconds(1f);

        // ⬆ плавно появиться
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            gameResultPanel.alpha = t;
            yield return null;
        }

        // ⏳ висит чуть-чуть
        yield return new WaitForSeconds(2f);

        // ⬇ плавное исчезновение
        while (t > 0f)
        {
            t -= Time.deltaTime;
            gameResultPanel.alpha = t;
            yield return null;
        }

        gameResultPanel.gameObject.SetActive(false);
    }
}
