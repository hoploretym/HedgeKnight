using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardButton : MonoBehaviour
{
    private int cardIndex;
    private HandManager handManager;
    private GameManager gameManager; // Добавляем ссылку на GameManager

    public TextMeshProUGUI cardNameText;
    public Image cardImage;
    private Button button;

public void Initialize(int index, HandManager hand)
{
    cardIndex = index;
    handManager = hand;

    if (handManager == null)
    {
        Debug.LogError($"Ошибка: `handManager` в `CardButton` == null! Индекс: {cardIndex}");
        return;
    }

    GetComponent<Button>().onClick.AddListener(OnCardClicked);
}


public void OnCardClicked()
{
    Debug.Log($"Карта {cardIndex} нажата!");
    handManager.RemoveCard(cardIndex);
}
}