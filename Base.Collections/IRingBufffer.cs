using System.Collections.Generic;
using System.Dynamic;

namespace Base.Collections
{
    public interface IRingBuffer<T> 
    {
        int Capacity { get; }
        int Count { get; }
        T this[int index] { get; }

        void Push(T value);
        void Reset();
    }
}