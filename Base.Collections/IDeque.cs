using System;
using System.Collections;
using System.Collections.Generic;

namespace Base.Collections
{
    public interface IDeque<T> : ICollection, IEnumerable<T>, ICloneable, IContains<T>
    {
        T this[int index] { get; }
        void Clear();
        void PushFront(T item);
        void PushBack(T item);
        T PopFront();
        T PopBack();
        T PeekFront();
        T PeekBack();
        T[] ToArray();
    }
}