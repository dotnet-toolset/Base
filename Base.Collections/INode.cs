using System.Collections.Generic;

namespace Base.Collections
{
    public interface INode<T> : IEnumerable<T> where T : class, INode<T>
    {
        T ParentNode { get; }
        T FirstChild { get; }
        T LastChild { get; }
        T PreviousSibling { get; }
        T NextSibling { get; }

        void Insert(T node, T before = null);
        bool Remove(T child);
    }

}
