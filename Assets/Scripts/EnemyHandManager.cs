using System.Collections.Generic;
using UnityEngine;

public class EnemyHandManager : MonoBehaviour
{
    private Deck deck;
    private List<Card> cardsInHand = new List<Card>();

    public void Initialize(Deck deckReference)
    {
        deck = deckReference;
        Debug.Log($"EnemyHandManager: deck установлен, карт в колоде: {deck.CardsCount()}");
    }

    public void DrawNewHand()
    {
        if (deck == null)
        {
            Debug.LogError("Ошибка: deck в EnemyHandManager == null! Проверь, задана ли колода.");
            return;
        }

        cardsInHand.Clear();

        for (int i = 0; i < 5; i++)
        {
            Card newCard = deck.DrawCard();
            if (newCard != null)
            {
                cardsInHand.Add(newCard);
            }
        }

        Debug.Log($"EnemyHandManager: Добрали {cardsInHand.Count} карт.");
    }

    public Card GetRandomCard()
    {
        if (cardsInHand.Count == 0) return null;
        return cardsInHand[Random.Range(0, cardsInHand.Count)];
    }

    public void RemoveCard(Card card)
    {
        if (cardsInHand.Contains(card))
        {
            Debug.Log($"EnemyHandManager: Удаляем карту {card.Name}");
            deck.AddToDiscard(card);
            cardsInHand.Remove(card);
        }
    }
}
