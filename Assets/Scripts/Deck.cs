using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour // Теперь это MonoBehaviour!
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

            cards.AddRange(discardPile);
            discardPile.Clear();
            Shuffle();
        }

        Card drawnCard = cards[0];
        cards.RemoveAt(0);
        return drawnCard;
    }

    public void DiscardCard(Card card)
    {
        discardPile.Add(card);
    }

    public int CardsCount()
    {
        return cards.Count;
    }

    public void FillDeck()
    {
        AddCard(new Card("Overhead Strike", 1, "Head"));
        AddCard(new Card("Thrust", 1, "Torso"));
        AddCard(new Card("Shield Slam", 1, "Torso"));
        AddCard(new Card("Pommel Strike", 1, "Head"));
        AddCard(new Card("Parry & Riposte", 1, "Arms"));
        AddCard(new Card("Low Cut", 1, "Legs"));
        AddCard(new Card("Half-Swording", 1, "Torso"));
        AddCard(new Card("Hook & Pull", 1, "Arms"));
        AddCard(new Card("Cross Guard Punch", 1, "Head"));
        AddCard(new Card("Mordhau Strike", 1, "Head"));
    }
}
