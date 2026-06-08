using Xunit;
using TheAdventure;
namespace Blackjack.Tests;

public class HandTests
{
    [Fact]
    public void CalculateScore_ShouldReturnSumOfCards_WithoutAces()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, CardValue.Ten));
        hand.AddCard(new Card(Suit.Spades, CardValue.Seven));

        
        Assert.Equal(17, hand.Score);
    }

    [Fact]
    public void CalculateScore_ShouldCountAceAs11_WhenNotBusting()
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, CardValue.Ace));
        hand.AddCard(new Card(Suit.Spades, CardValue.Nine));

       
        Assert.Equal(20, hand.Score);
    }

    [Fact]
    public void CalculateScore_ShouldCountAceAs1_WhenOtherwiseBusting()
    {
        // Arrange
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, CardValue.Ace));
        hand.AddCard(new Card(Suit.Spades, CardValue.Ten));
        hand.AddCard(new Card(Suit.Clubs, CardValue.Eight));

        Assert.Equal(19, hand.Score);
    }
}