namespace TheAdventure.Models;

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum CardValue
{
    Two = 2, 
    Three = 3, 
    Four = 4, 
    Five = 5, 
    Six = 6, 
    Seven = 7, 
    Eight = 8, 
    Nine = 9, 
    Ten = 10,
    Jack = 11, 
    Queen = 12, 
    King = 13, 
    Ace = 14
}

public class Card
{
    public Suit Suit { get; }
    public CardValue Value { get; }

    public Card(Suit suit, CardValue value)
    {
        Suit = suit;
        Value = value;
    }
}