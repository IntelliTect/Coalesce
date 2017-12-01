using System.Collections.Generic;
using System.Collections;

namespace IntelliTect.Coalesce.Utilities
{
    public struct ReadOnlyHashSet<T> : IReadOnlyCollection<T>
    {
        private readonly HashSet<T> set;

        internal ReadOnlyHashSet(HashSet<T> set)
        {
            this.set = set;
        }

        public int Count => set.Count;

        public IEnumerator<T> GetEnumerator() => ((IReadOnlyCollection<T>)set).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyCollection<T>)set).GetEnumerator();

        public bool Contains(T item) => set.Contains(item);
    }
}
