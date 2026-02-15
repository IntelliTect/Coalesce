using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Utilities;

internal class ConcurrentHashSet<T> : IReadOnlyCollection<T>
    where T : notnull
{
    private readonly ConcurrentDictionary<T, bool> _dict = new();

    public int Count => throw new System.NotImplementedException();

    public bool Add(T item) => _dict.TryAdd(item, true);

    public void Remove(T item) => _dict.Remove(item, out _);

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items) Add(item);
    }

    public void RemoveRange(IEnumerable<T> items)
    {
        foreach (var item in items) Remove(item);
    }

    public bool Contains(T? item) => item is null ? false : _dict.TryGetValue(item, out _);

    public IEnumerator<T> GetEnumerator() => _dict.Select(p => p.Key).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dict.Select(p => p.Key).GetEnumerator();
}
