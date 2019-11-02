using System.Collections.Generic;

namespace Base.Collections
{
    public interface IReadOnlySet<T> : IReadOnlyCollection<T>, IContains<T>
    {
        bool IsEmpty { get; }
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
    }
}
