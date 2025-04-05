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
            Debug.LogError(
                "[HandManager] Ошибка: deck в HandManager == null! Проверь, задана ли колода в инспекторе."
            );
            return;
        }

        // ✅ Если в руке ещё есть карты, НЕ обновляем
        if (cardsInHand.Count > 0)
            return;

        Debug.Log("[HandManager] Рука пуста, обновляем!");

        ClearHand();
        cardsInHand.Clear();

        for (int i = 0; i < 5; i++)
        {
            Card newCard = deck.DrawCard();
            if (newCard != null)
            {
                cardsInHand.Add(newCard);
                CreateCardUI(newCard);
            }
        }

        Debug.Log($"[HandManager] Всего карт в руке после обновления: {cardsInHand.Count}");
    }

    private void ClearHand()
    {
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateCardUI(Card card)
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
        cardButton.Initialize(card, this); // ✅ Передаем только 2 аргумента
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

            // ✅ НЕ вызываем DrawNewHand() после удаления карты!
        }
    }

    public Card GetRandomCard()
    {
        if (cardsInHand.Count == 0)
            return null;
        return cardsInHand[Random.Range(0, cardsInHand.Count)];
    }

    public Card DrawOneCard()
    {
        Character character = GameManager.Instance.playerController.GetCharacter();

        if (character != null && character.ShouldDrawOneLessCard())
        {
            Debug.Log("[HandManager] Эффект Broken Helmet: карта не добирается в этот ход.");
            return null;
        }

        Card newCard = deck.DrawCard();
        if (newCard != null)
        {
            cardsInHand.Add(newCard);
            CreateCardUI(newCard);
        }

        return newCard;
    }

    public void UpdateHandUI()
    {
        foreach (Transform child in handPanel)
        {
            CardButton cardButton = child.GetComponent<CardButton>();
            if (cardButton != null)
            {
                cardButton.UpdateDescription();
                cardButton.UpdateEnergyCost();
            }
        }
    }
}
