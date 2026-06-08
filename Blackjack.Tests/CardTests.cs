using Xunit;
using TheAdventure.Models; 

namespace Blackjack.Tests;

public class CardTests
{
    [Fact]
    public void Card_ShouldHaveCorrectSuitAndValue()
    {
        var card = new Card(Suit.Hearts, CardValue.Ten);

        Assert.Equal(Suit.Hearts, card.Suit);
        Assert.Equal(CardValue.Ten, card.Value);
    }
}