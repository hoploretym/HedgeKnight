using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardButton : MonoBehaviour
{
    private int cardIndex;
    private HandManager handManager;

    public TextMeshProUGUI cardNameText;
    public Image cardImage;
    private Button button;
    public Card Card { get; private set; }

    public int CardIndex => cardIndex; // Свойство для индекса карты

    public void Initialize(Card card, int index, HandManager hand)
    {
        if (card == null)
        {
            Debug.LogError($"[CardButton] Ошибка: передана NULL-карта в Initialize! Индекс: {index}");
            return;
        }

        cardIndex = index;
        handManager = hand;
        Card = card;

        GetComponent<Button>().onClick.AddListener(OnCardClicked);

// 🔴 Красная рамка для атакующих карт
    if (card.Type == CardType.Attack)
    {
        cardImage.color = new Color32(255, 0, 0, 255);
    }
    // 🔵 Синяя рамка для защитных карт
    else if (card.Type == CardType.Defense)
    {
        cardImage.color = new Color32(0, 0, 255, 255);
    }

        Debug.Log($"[CardButton] Карта {card.Name} (Index {index}) инициализирована. Тип: {card.Type}");
    }

    public void OnCardClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[CardButton] Ошибка: GameManager.Instance == null!");
            return;
        }

        Debug.Log($"[DEBUG] Клик по карте {CardIndex}, передаём в GameManager.");
        GameManager.Instance.PlayerSelectCard(CardIndex);
        Debug.Log($"[DEBUG] Карта {CardIndex} передана в GameManager.");
    }

    public void UpdateIndex(int newIndex)
    {
        cardIndex = newIndex;
    }
}
