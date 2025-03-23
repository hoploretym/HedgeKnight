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
        AddCard(new Card("atk_01", "Overhead Strike", CardType.Attack, "Head", 5, 1));
        AddCard(new Card("atk_02", "Thrust", CardType.Attack, "Torso", 5, 1));
        AddCard(new Card("atk_03", "Low Cut", CardType.Attack, "Legs", 5, 1));
        AddCard(new Card("atk_04", "Mordhau Strike", CardType.Attack, "Arms", 5, 1));

        AddCard(new Card("spc_01", "Pommel Strike", CardType.Special, "Head", 5, 1));
        AddCard(new Card("spc_02", "Shield Slam", CardType.Special, "Torso", 5, 1));
        AddCard(new Card("spc_03", "Parry & Riposte", CardType.Special, "Arms", 5, 1));
        AddCard(new Card("spc_04", "Hook & Pull", CardType.Special, "Legs", 5, 1));

        AddCard(
            new Card(
                "def_01",
                "High Guard",
                CardType.Defense,
                "Head",
                -5,
                0,
                new string[] { "Overhead Strike", "Pommel Strike", "Cross Guard Punch" }
            )
        );
        AddCard(
            new Card(
                "def_02",
                "Brace Stance",
                CardType.Defense,
                "Torso",
                -5,
                0,
                new string[] { "Thrust", "Shield Slam", "Half-Swording" }
            )
        );
        AddCard(
            new Card(
                "def_03",
                "Guarded Step Back",
                CardType.Defense,
                "Legs",
                -5,
                0,
                new string[] { "Low Cut", "Hook & Pull", "Mordhau Strike" }
            )
        );
        AddCard(
            new Card(
                "def_04",
                "Strong Grip",
                CardType.Defense,
                "Arms",
                -5,
                0,
                new string[] { "Parry & Riposte", "Thrust", "Hook & Pull" }
            )
        );
    }
}
