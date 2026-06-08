using System.Collections.Generic;

namespace TheAdventure.Models;

public class Deck<T>
{
    private List<T> _items;

    public int Count => _items.Count;

    public Deck(IEnumerable<T> items)
    {
        _items = new List<T>(items);
    }
}