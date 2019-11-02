using System;
using System.Collections;
using System.Collections.Generic;

namespace Base.Collections
{
    public class ListSegment<T> : IReadOnlyList<T>
    {
        private readonly int _start, _count;
        private readonly IReadOnlyList<T> _list;

        public ListSegment(IReadOnlyList<T> list, int start, int count)
        {
            if (start < 0 || start > list.Count) throw new ArgumentException(nameof(start));
            if (count < 0) count = list.Count - start;
            if (start + count > list.Count) throw new ArgumentException(nameof(count));
            _list = list;
            _start = start;
            _count = count;
        }

        private class Enumerator : IEnumerator<T>
        {
            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_count == 0) return false;
                _current = _list._list[_index];
                _count--;
                _index++;
                return true;
            }

            public void Reset()
            {
                _count = _list._count;
                _index = _list._start;
            }

            object IEnumerator.Current => Current;

            public T Current => _current;

            private readonly ListSegment<T> _list;
            private int _count;
            private int _index;
            private T _current;

            public Enumerator(ListSegment<T> list)
            {
                _list = list;
                Reset();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _count;

        public T this[int index] => _list[_start + index];
    }
}