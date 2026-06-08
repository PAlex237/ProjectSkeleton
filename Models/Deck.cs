using System;
using System.Collections.Generic;

namespace TheAdventure.Models;

public class Deck<T>
{
    private List<T> _items;
    private static readonly Random _random = new Random();

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

    public void Shuffle()
    {
        int n = _items.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            
            T value = _items[k];
            _items[k] = _items[n];
            _items[n] = value;
        }
    }
}