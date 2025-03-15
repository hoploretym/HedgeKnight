using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public Transform handPanel;
    public GameObject cardPrefab;
    public List<Card> cardsInHand = new List<Card>();
    private Deck deck;
    private List<int> selectedCardsIndexes = new List<int>();

    public void Initialize(Deck deckReference)
    {
        deck = deckReference;
        Debug.Log($"HandManager: deck установлен, карт в колоде: {deck.CardsCount()}");
    }

    public void DrawNewHand()
{
    if (deck == null)
    {
        Debug.LogError("[HandManager] Ошибка: deck в HandManager == null! Проверь, задана ли колода в инспекторе.");
        return;
    }

    Debug.Log($"[HandManager] Добираем карты. Количество карт в колоде: {deck.CardsCount()}");

    ClearHand();
    cardsInHand.Clear();
    selectedCardsIndexes.Clear();

    for (int i = 0; i < 5; i++)
    {
        Card newCard = deck.DrawCard();
        if (newCard != null)
        {
            cardsInHand.Add(newCard);
            Debug.Log($"[HandManager] Добавлена карта в руку: {newCard.Name}");
            CreateCardUI(newCard, i);
        }
        else
        {
            Debug.LogWarning($"[HandManager] Карта {i} не была добрана, колода пуста.");
        }
    }

    Debug.Log($"[HandManager] Всего карт в руке после добора: {cardsInHand.Count}");
}

    private void ClearHand()
    {
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateCardUI(Card card, int index)
{
    if (cardPrefab == null)
    {
        Debug.LogError("Ошибка: `cardPrefab` не назначен в HandManager!");
        return;
    }

    if (handPanel == null)
    {
        Debug.LogError("Ошибка: `handPanel` не назначен в HandManager!");
        return;
    }

    GameObject cardObj = Instantiate(cardPrefab, handPanel);
    cardObj.transform.SetParent(handPanel, false);
    cardObj.transform.localScale = Vector3.one;

    CardButton cardButton = cardObj.GetComponent<CardButton>();
    if (cardButton == null)
    {
        Debug.LogError("Ошибка: У `cardPrefab` нет `CardButton`!");
        return;
    }

    if (cardButton.cardNameText == null)
    {
        Debug.LogError("Ошибка: `cardNameText` не назначен в `CardButton`!");
        return;
    }

    cardButton.cardNameText.text = card.Name;
    cardButton.Initialize(card, index, this); // ✅ Передаём `this` как `HandManager`
}


    public void RemoveCard(Card card)
    {
        if (cardsInHand.Contains(card))
        {
            Debug.Log($"Удаляем карту: {card.Name}");
            deck.AddToDiscard(card);
            cardsInHand.Remove(card);

            foreach (Transform child in handPanel)
            {
                CardButton cardButton = child.GetComponent<CardButton>();
                if (cardButton != null && cardButton.Card == card)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
    }

    // ✅ **Добавляем этот метод в КОНЕЦ КЛАССА перед `}`**
    public Card GetRandomCard()
    {
        if (cardsInHand.Count == 0) return null;
        return cardsInHand[Random.Range(0, cardsInHand.Count)];
    }
}
