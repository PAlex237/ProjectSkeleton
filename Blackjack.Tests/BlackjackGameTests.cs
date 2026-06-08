using Xunit;
using TheAdventure.Models;
namespace Blackjack.Tests;

public class BlackjackGameTests
{
    [Fact]
    public void PlaceBet_ShouldDecreasePlayerBalance()
    {
        var game = new BlackjackGame();
        game.PlayerBalance = 100;

        game.PlaceBet(20);

        Assert.Equal(80, game.PlayerBalance);
        Assert.Equal(20, game.CurrentBet);
    }

    [Fact]
    public void PlaceBet_ShouldThrowException_WhenBetExceedsBalance()
    {
        var game = new BlackjackGame();
        game.PlayerBalance = 50;

        Assert.Throws<InsufficientFundsException>(() => game.PlaceBet(60));
    }
}