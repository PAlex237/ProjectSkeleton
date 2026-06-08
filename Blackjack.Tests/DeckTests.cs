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

[Fact]
    public void Draw_ShouldReturnTopItem_AndDecreaseCount()
    {
        // Arrange
        var initialItems = new List<string> { "Card1", "Card2" };
        var deck = new Deck<string>(initialItems);

        // Act
        // Tragem o carte (ar trebui să fie prima din listă, adică "Card1")
        var drawnItem = deck.Draw();

        // Assert
        Assert.Equal("Card1", drawnItem);
        // Ne așteptăm să mai rămână doar o carte în pachet
        Assert.Equal(1, deck.Count);
    }

    [Fact]
    public void Draw_ShouldThrowEmptyDeckException_WhenDeckIsEmpty()
    {
        // Arrange
        // Creăm un pachet complet gol
        var deck = new Deck<string>(new List<string>());

        // Act & Assert
        // Verificăm că apelarea metodei Draw aruncă excepția noastră custom
        Assert.Throws<EmptyDeckException>(() => deck.Draw());
    }
    [Fact]
    public void Shuffle_ShouldChangeTheOrderOfItems()
    {
        var initialItems = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        var deck = new Deck<string>(initialItems);

        deck.Shuffle();

        var drawnItems = new List<string>();
        while(deck.Count > 0) 
        {
            drawnItems.Add(deck.Draw());
        }

        Assert.Equal(10, drawnItems.Count);
        Assert.NotEqual(initialItems, drawnItems);
    }
}