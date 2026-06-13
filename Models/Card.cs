namespace TheAdventure;

public enum Suit
{
    Hearts = 0,
    Diamonds = 1,
    Clubs = 2,
    Spades = 3
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
// AI-generated
    // Aflăm pe ce coloană se află cartea (Axa X)
    public int GetSpriteColumn()
    {
        // Asul e pe coloana 0
        if (Value == CardValue.Ace) 
        {
            return 0;
        }
        // Restul cărților sunt pur și simplu valoarea lor minus 1
        // Ex: Cartea 2 -> Coloana 1. Popa (13) -> Coloana 12.
        return (int)Value - 1;
    }

    // Aflăm pe ce rând se află culoarea (Axa Y)
    public int GetSpriteRow()
    {
        // Formula magică: Hearts(0)->3, Diamonds(1)->2, Clubs(2)->1, Spades(3)->0
        return 3 - (int)Suit;
    }
    // end AI-generated
}