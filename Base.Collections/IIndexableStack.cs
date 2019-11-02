using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Collections
{
    public interface IIndexableStack<T> : IEnumerable<T>
    {
        bool IsEmpty { get; }
        int Count { get; }

        void Clear();
        void Push(T item);
        T Pop();
        T Peek(bool returnDefault = false);
        T Peek(int index, bool returnDefault = false);
    }
}
