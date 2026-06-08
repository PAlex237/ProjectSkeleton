using Xunit;
using System.Collections.Generic;
using TheAdventure.Models; 

namespace Blackjack.Tests;

public class DeckTests
{
    [Fact]
    public void Deck_ShouldInitializeWithItems_AndReportCorrectCount()
    {
        var initialItems = new List<string> { "Item1", "Item2", "Item3" };
        
        var deck = new Deck<string>(initialItems);

        Assert.Equal(3, deck.Count);
    }
}