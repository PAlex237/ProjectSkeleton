using System;

namespace TheAdventure;
public class EmptyDeckException : Exception
{
    public EmptyDeckException(string message = "Cannot draw from an empty deck.") 
        : base(message)
    {
    }
}