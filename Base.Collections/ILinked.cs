using System.Collections.Generic;

namespace Base.Collections
{
    public interface ILinked<T> : IEnumerable<T>
        where T : class, ILinked<T>
    {
        T Next { get; }
        T Previous { get; }
    }

    public interface ILinkedMutable<T> : ILinked<T>
        where T : class, ILinkedMutable<T>
    {
        void InsertAfter(T node);
        void InsertBefore(T node);
        void Remove();
    }
}
