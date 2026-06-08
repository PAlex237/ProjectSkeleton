namespace TheAdventure;
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
    public GameResult DetermineWinner(Hand playerHand, Hand dealerHand)
    {
        int playerScore = playerHand.Score;
        int dealerScore = dealerHand.Score;

        if (playerScore > 21)
        {
            return GameResult.DealerWins; // Player busts
        }
        else if (dealerScore > 21)
        {
            return GameResult.PlayerWins; // Dealer busts
        }
        else if (playerScore > dealerScore)
        {
            return GameResult.PlayerWins;
        }
        else if (dealerScore > playerScore)
        {
            return GameResult.DealerWins;
        }
        else
        {
            return GameResult.Push; // Tie
        }
    }

}
