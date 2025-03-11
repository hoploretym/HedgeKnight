using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public Transform handPanel;
    public GameObject cardPrefab;
    public List<Card> cardsInHand = new List<Card>();
    private Deck deck;
    private int selectedCardIndex = -1; // Индекс выбранной карты

    public void Initialize(Deck deckReference)
    {
        deck = deckReference;
        Debug.Log($"HandManager: deck установлен, карт в колоде: {deck.CardsCount()}");
    }

    public void DrawNewHand()
    {
        if (deck == null)
        {
            Debug.LogError("Ошибка: deck в HandManager == null! Проверь, задана ли колода в инспекторе.");
            return;
        }

        Debug.Log($"Раздача карт. Количество карт в колоде: {deck.CardsCount()}");
        Debug.Log($"{gameObject.name} раздает карты в {handPanel.name}");


        ClearHand();
        cardsInHand.Clear();

        for (int i = 0; i < 5; i++)
        {
            Card newCard = deck.DrawCard();
            if (newCard != null)
            {
                cardsInHand.Add(newCard);
                CreateCardUI(newCard, i);
            }
        }
    }

public void RemoveCard(int index)
{
    if (index >= 0 && index < cardsInHand.Count)
    {
        Debug.Log($"Удаляем карту с индексом: {index} ({cardsInHand[index].Name})");

        cardsInHand.RemoveAt(index); // Удаляем карту из списка
        ClearHand(); // Чистим UI
        RedrawHand(); // Перерисовываем карты
    }
}

private void RedrawHand()
{
    Debug.Log("Перерисовка руки. Осталось карт: " + cardsInHand.Count);
    
    for (int i = 0; i < cardsInHand.Count; i++)
    {
        CreateCardUI(cardsInHand[i], i); // Генерируем карту с новым индексом
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
        cardButton.Initialize(index, this);
    }

    public void SelectCard(int index)
    {
        if (selectedCardIndex == index) // Если та же карта – отменяем выбор
        {
            selectedCardIndex = -1;
            return;
        }

        selectedCardIndex = index;
        Debug.Log($"Выбрана карта с индексом: {index}");

        // Сбрасываем выделение у всех карт
        foreach (Transform child in handPanel)
        {
            Image img = child.GetComponent<Image>();
            if (img != null) img.color = Color.white;
        }

        // Выделяем выбранную карту
        if (index >= 0 && index < handPanel.childCount)
        {
            Transform selectedCard = handPanel.GetChild(index);
            Image selectedImg = selectedCard.GetComponent<Image>();
            if (selectedImg != null) selectedImg.color = Color.yellow;
        }
    }

    public int GetSelectedCardIndex()
    {
        return selectedCardIndex;
    }

    private void ClearHand()
    {
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public Card GetRandomCard() // Противник случайно выбирает карту
    {
        if (cardsInHand.Count == 0) return null;
        return cardsInHand[Random.Range(0, cardsInHand.Count)];
    }
}
