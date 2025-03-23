using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardButton : MonoBehaviour
{
    private HandManager handManager;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI energyCostText;
    public Image CardImage;
    public Outline cardOutline; // ← ВОТ ЗДЕСЬ!
    private Button button;

    public TextMeshProUGUI targetBodyPartText;

    private Color originalColor;
    public Card Card { get; private set; }

    public void Initialize(Card card, HandManager hand)
    {
        if (card == null)
        {
            Debug.LogError("[CardButton] Ошибка: передана NULL-карта в Initialize!");
            return;
        }

        handManager = hand;
        Card = card;

        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnCardClicked);

        if (cardNameText != null)
            cardNameText.text = card.Name;

        if (energyCostText != null)
        {
            string prefix = Card.Type == CardType.Defense ? "+" : "-";
            energyCostText.text = $"{prefix}{Mathf.Abs(Card.EnergyCost)}";
        }

        if (targetBodyPartText != null)
            targetBodyPartText.text = card.TargetBodyPart.ToString();

        UpdateCardAppearance();

        if (cardOutline != null)
            cardOutline.enabled = false;
    }

    public void SetOutline(bool enabled)
    {
        if (cardOutline != null)
        {
            cardOutline.enabled = enabled;
            Debug.Log($"Outline {(enabled ? "включён" : "выключен")} на карте: {Card.Name}");
        }
        else
            Debug.LogError("cardOutline не назначен!");
    }

    private void UpdateCardAppearance()
    {
        if (CardImage == null)
            return;

        if (Card.Type == CardType.Attack)
            originalColor = new Color32(255, 0, 0, 255);
        else if (Card.Type == CardType.Defense)
            originalColor = new Color32(0, 0, 255, 255);
        else if (Card.Type == CardType.Special)
            originalColor = new Color32(255, 165, 0, 255);

        CardImage.color = originalColor;
    }

    public void OnCardClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[CardButton] Ошибка: GameManager.Instance == null!");
            return;
        }

        int index = handManager.cardsInHand.IndexOf(Card);

        if (index == -1)
        {
            Debug.LogError($"[CardButton] Ошибка: Карта {Card.Name} не найдена в руке!");
            return;
        }

        Character player = GameManager.Instance.playerController.GetCharacter();

        if (Card.Type != CardType.Defense && player.Energy < Card.EnergyCost)
        {
            GameManager.Instance.gameUI.ShowFloatingMessage("Недостаточно энергии!");
            return;
        }

        Debug.Log($"[DEBUG] Клик по карте {Card.Name} (индекс {index})");
        GameManager.Instance.PlayerSelectCard(index);
    }
}
