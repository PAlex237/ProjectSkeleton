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

    public T Draw()
    {
        if (_items.Count == 0)
        {
            throw new EmptyDeckException();
        }

        T item = _items[0];
        _items.RemoveAt(0);
        return item;
    }
}