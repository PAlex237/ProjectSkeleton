using System;

namespace TheAdventure.Models;

public class EmptyDeckException : Exception
{
    public EmptyDeckException(string message = "Cannot draw from an empty deck.") 
        : base(message)
    {
    }
}