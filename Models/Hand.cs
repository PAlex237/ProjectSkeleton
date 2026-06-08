using System.Collections.Generic;
using System.Linq;

namespace TheAdventure.Models;

public class Hand
{
    private List<Card> _cards = new List<Card>();

    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();

    public void AddCard(Card card)
    {
        _cards.Add(card);
    }

    public int Score
    {
        get
        {
            int totalScore = _cards.Sum(c => GetCardValue(c.Value));
            
            int acesCount = _cards.Count(c => c.Value == CardValue.Ace);

            while (totalScore > 21 && acesCount > 0)
            {
                totalScore -= 10;
                acesCount--;
            }

            return totalScore;
        }
    }

    // Funcție ajutătoare pentru a converti Enum-ul în valoare de Blackjack
    private int GetCardValue(CardValue value)
    {
        if (value == CardValue.Jack || value == CardValue.Queen || value == CardValue.King)
        {
            return 10;
        }
        if (value == CardValue.Ace)
        {
            return 11;
        }
        return (int)value;
    }
}