using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardButton : MonoBehaviour
{
    private HandManager handManager;
    public TextMeshProUGUI cardNameText;
    public Image cardImage;
    private Button button;
    public Card Card { get; private set; }

    public void Initialize(Card card, HandManager hand)
    {
        if (card == null)
        {
            Debug.LogError($"[CardButton] Ошибка: передана NULL-карта в Initialize!");
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

        UpdateCardAppearance();
    }

    private void UpdateCardAppearance()
    {
        if (cardImage == null) return;

        if (Card.Type == CardType.Attack)
        {
            cardImage.color = new Color32(255, 0, 0, 255);
        }
        else if (Card.Type == CardType.Defense)
        {
            cardImage.color = new Color32(0, 0, 255, 255);
        }
    }

    public void OnCardClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[CardButton] Ошибка: GameManager.Instance == null!");
            return;
        }

        // ✅ Теперь мы получаем правильный индекс карты из массива
        int index = handManager.cardsInHand.IndexOf(Card);

        if (index == -1)
        {
            Debug.LogError($"[CardButton] Ошибка: Карта {Card.Name} не найдена в руке!");
            return;
        }

        Debug.Log($"[DEBUG] Клик по карте {Card.Name} (индекс {index})");
        GameManager.Instance.PlayerSelectCard(index);
    }
}
