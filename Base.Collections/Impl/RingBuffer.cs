using System;
using System.Diagnostics;

namespace Base.Collections.Impl
{
    public class RingBuffer<T> : IRingBuffer<T>
    {
        private int _head, _size;
        private readonly T[] _list;

        public int Count => _size;
        public int Capacity => _list.Length;
        public T this[int index] => _list[Index(index)];

        public RingBuffer(int cap)
        {
            _list = new T[cap];
        }

        public void Push(T item)
        {
            _head = ModuloAdd(_head, -1, _list.Length);
            _list[_head] = item;
            _size = Math.Min(_size + 1, _list.Length);
        }

        public void Reset()
        {
            _head = 0;
            _size = 0;
        }

        private int Index(int index)
        {
            Debug.Assert(index >= 0 && index < _size);
            return ModuloAdd(_head, index, _list.Length);
        }

        static int ModuloAdd(int x, int y, int mod)
        {
            var sum = x + y;
            Debug.Assert(0 <= x && x < mod && -mod <= y && y <= mod);
            if (sum >= mod)
                sum -= mod;
            if (sum < 0)
                sum += mod;
            return sum;
        }
    }
}