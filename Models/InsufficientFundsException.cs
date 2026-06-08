using System;

namespace TheAdventure;
public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(string message = "Not enough funds to place this bet.") 
        : base(message)
    {
    }
}