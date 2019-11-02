using System.Collections.Generic;

namespace Base.Collections
{
    public interface IReadOnlySortedSet<T> : IReadOnlySet<T>
    {
        T Max { get; }
        T Min { get; }
        IComparer<T> Comparer { get; }
        IReadOnlySortedSet<T> GetViewBetween(T lowerValue, T upperValue);
        bool TryGetValue(T equalValue, out T actualValue);
    }
}