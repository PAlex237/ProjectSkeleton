namespace TheAdventure.Models;

public class BlackjackGame
{
    public int PlayerBalance { get;  set; }
    public int CurrentBet { get; private set; }
    
public BlackjackGame(int startingBalance = 100)
    {
        
    }

    public void PlaceBet(int amount)
    {
        if (amount > PlayerBalance)
        {
            throw new InsufficientFundsException();
        }

        PlayerBalance -= amount;
        CurrentBet = amount;
    }
}
