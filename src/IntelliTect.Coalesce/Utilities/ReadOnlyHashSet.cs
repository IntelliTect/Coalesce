using System.Collections;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Utilities;

public readonly struct ReadOnlyHashSet<T> : IReadOnlyCollection<T>
    where T : notnull
{
    private readonly ConcurrentHashSet<T> set;

    internal ReadOnlyHashSet(ConcurrentHashSet<T> set)
    {
        this.set = set;
    }

    public int Count => set.Count;

    public IEnumerator<T> GetEnumerator() => set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => set.GetEnumerator();

    public bool Contains(T? item) => set.Contains(item);
}
