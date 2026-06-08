using System;

namespace TheAdventure.Models;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(string message = "Not enough funds to place this bet.") 
        : base(message)
    {
    }
}