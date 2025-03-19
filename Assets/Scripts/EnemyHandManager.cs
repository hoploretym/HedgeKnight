using System.Collections.Generic;
using UnityEngine;

public class EnemyHandManager : MonoBehaviour
{
    private Deck deck;
    public List<Card> cardsInHand = new List<Card>();

    public void Initialize(Deck deckReference)
    {
        deck = deckReference;
        Debug.Log($"EnemyHandManager: deck установлен, карт в колоде: {deck.CardsCount()}");
    }

public void DrawNewHand()
{
    if (deck == null)
    {
        Debug.LogError("[EnemyHandManager] Ошибка: deck в EnemyHandManager == null!");
        return;
    }

    // ✅ Если рука НЕ пуста, ничего не делаем
    if (cardsInHand.Count > 0) return;

    Debug.Log("[EnemyHandManager] Рука пуста, обновляем!");

    cardsInHand.Clear();

    for (int i = 0; i < 5; i++)
    {
        Card newCard = deck.DrawCard();
        if (newCard != null)
        {
            cardsInHand.Add(newCard);
        }
    }

    Debug.Log($"[EnemyHandManager] Всего карт в руке после обновления: {cardsInHand.Count}");
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

    // ✅ Публичное свойство для проверки количества карт в руке
    public int CardsInHandCount => cardsInHand.Count;
}
