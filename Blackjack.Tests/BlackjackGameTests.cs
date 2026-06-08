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
    [Fact]
    public void PlaceBet_ShouldAllowBetEqualToBalance()
    {
        var game = new BlackjackGame();
        game.PlayerBalance = 50;

        game.PlaceBet(50);

        Assert.Equal(0, game.PlayerBalance);
        Assert.Equal(50, game.CurrentBet);
    }
    [Fact]
    public void DetermineWinner_ShouldReturnPlayer_WhenPlayerHasHigherScore()
    {
        var game = new BlackjackGame();
        var playerHand = new Hand();
        playerHand.AddCard(new Card(Suit.Hearts, CardValue.Ten));
        playerHand.AddCard(new Card(Suit.Spades, CardValue.Seven));

        var dealerHand = new Hand();
        dealerHand.AddCard(new Card(Suit.Clubs, CardValue.Nine));
        dealerHand.AddCard(new Card(Suit.Diamonds, CardValue.Six));

        var result = game.DetermineWinner(playerHand, dealerHand);

        Assert.Equal(GameResult.PlayerWins, result);
    }
    [Fact]
    public void DetermineWinner_ShouldReturnDealer_WhenDealerHasHigherScore()
    {
        var game = new BlackjackGame();
        var playerHand = new Hand();
        playerHand.AddCard(new Card(Suit.Hearts, CardValue.Ten));
        playerHand.AddCard(new Card(Suit.Spades, CardValue.Seven));

        var dealerHand = new Hand();
        dealerHand.AddCard(new Card(Suit.Clubs, CardValue.King));
        dealerHand.AddCard(new Card(Suit.Diamonds, CardValue.Queen));

        var result = game.DetermineWinner(playerHand, dealerHand);

        Assert.Equal(GameResult.DealerWins, result);
    }
}