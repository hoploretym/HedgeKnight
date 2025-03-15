using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    private List<Card> discardPile = new List<Card>();

    void Start()
    {
        FillDeck();
        Shuffle();
    }

    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    public void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(0, cards.Count);
            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                Debug.Log("Колода и сброс пусты! Нечего добирать.");
                return null;
            }

            // Перемешиваем сброс в новую колоду
            cards.AddRange(discardPile);
            discardPile.Clear();
            Shuffle();
            Debug.Log("Перемешиваем сброс и создаем новую колоду.");
        }

        Card drawnCard = cards[0];
        cards.RemoveAt(0);
        return drawnCard;
    }

    public void DiscardCard(Card card)
    {
        discardPile.Add(card);
    }

    public void AddToDiscard(Card card)
    {
        discardPile.Add(card);
        Debug.Log($"Карта {card.Name} отправлена в сброс.");
    }

    public int CardsCount()
    {
        return cards.Count;
    }

public void FillDeck()
{
    AddCard(new Card("Overhead Strike", 1, "Head", 3, CardType.Attack));
    AddCard(new Card("Thrust", 1, "Torso", 2, CardType.Attack));
    AddCard(new Card("Shield Slam", 1, "Torso", 2, CardType.Attack));
    AddCard(new Card("Pommel Strike", 1, "Head", 1, CardType.Attack));
    AddCard(new Card("Parry & Riposte", 1, "Arms", 2, CardType.Attack));
    AddCard(new Card("Low Cut", 1, "Legs", 1, CardType.Attack));
    AddCard(new Card("Half-Swording", 1, "Torso", 3, CardType.Attack));
    AddCard(new Card("Hook & Pull", 1, "Arms", 2, CardType.Attack));
    AddCard(new Card("Cross Guard Punch", 1, "Head", 1, CardType.Attack));
    AddCard(new Card("Mordhau Strike", 1, "Head", 4, CardType.Attack));

    AddCard(new Card("High Guard", 1, "Arms", 0, CardType.Defense, new string[] { "Overhead Strike", "Pommel Strike", "Cross Guard Punch" }));
    AddCard(new Card("Brace Stance", 1, "Torso", 0, CardType.Defense, new string[] { "Thrust", "Shield Slam", "Half-Swording" }));
    AddCard(new Card("Guarded Step Back", 1, "Legs", 0, CardType.Defense, new string[] { "Low Cut", "Hook & Pull", "Mordhau Strike" }));
    AddCard(new Card("Strong Grip", 1, "Arms", 0, CardType.Defense, new string[] { "Parry & Riposte", "Thrust", "Hook & Pull" }));
    AddCard(new Card("Head Tilt & Angle", 1, "Head", 0, CardType.Defense, new string[] { "Pommel Strike", "Cross Guard Punch", "Mordhau Strike" }));
}
}
