using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardButton : MonoBehaviour
{
    private HandManager handManager;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI energyCostText;
    public Image cardImage;
    private Button button;
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
        {
            cardNameText.text = card.Name;
        }

        if (energyCostText != null)
        {
            string prefix = Card.Type == CardType.Defense ? "+" : "-";
            energyCostText.text = $"{prefix}{Mathf.Abs(Card.EnergyCost)}";
        }

        UpdateCardAppearance();
    }

    private void UpdateCardAppearance()
    {
        if (cardImage == null)
            return;

        if (Card.Type == CardType.Attack)
        {
            cardImage.color = new Color32(255, 0, 0, 255);
        }
        else if (Card.Type == CardType.Defense)
        {
            cardImage.color = new Color32(0, 0, 255, 255);
        }
        else if (Card.Type == CardType.Special)
        {
            cardImage.color = new Color32(255, 165, 0, 255);
        }
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

        // 🧠 Проверка энергии
        if (Card.Type != CardType.Defense && player.Energy < Card.EnergyCost)
        {
            GameManager.Instance.gameUI.ShowFloatingMessage("Недостаточно энергии!");
            return;
        }

        Debug.Log($"[DEBUG] Клик по карте {Card.Name} (индекс {index})");
        GameManager.Instance.PlayerSelectCard(index);
    }
}
