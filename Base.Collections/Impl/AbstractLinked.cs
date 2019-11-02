using System;
using System.Collections.Generic;

namespace Base.Collections.Impl
{
    public abstract class AbstractLinked<T> :  ILinkedMutable<T> where T : class, ILinkedMutable<T>
    {
        private AbstractLinked<T> _next, _prev;

        public T Next => (T)(object)_next;
        public T Previous => (T)(object)_prev;

        public void InsertAfter(T node)
        {
            if (!(node is AbstractLinked<T> n)) return;
            if (n._next != null || n._prev != null) throw new ArgumentException("this item is already member of other linked list");
            n._next = _next;
            n._prev = this;
            _next = n;
        }

        public void InsertBefore(T node)
        {
            if (!(node is AbstractLinked<T> n)) return;
            if (n._next != null || n._prev != null) throw new ArgumentException("this item is already member of other linked list");
            n._prev = _prev;
            n._next = this;
            _prev = n;
        }

        public void Remove()
        {
            if (_next == null && _prev == null) throw new ArgumentException("this item is not a member of a linked list");
            if (_prev != null)
            {
                _prev._next = _next;
                _prev = null;
            }
            if (_next != null)
            {
                _next._prev = _prev;
                _next = null;
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new LinkedEnumerator<T>(this);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new LinkedEnumerator<T>(this);
        }

        #endregion
    }

    public class LinkedEnumerator<T> : IEnumerator<T> where T : class, ILinked<T>
    {
        private readonly T _first;
        private bool _finished;
        private T _current;

        public LinkedEnumerator(ILinked<T> node)
        {
            _first = node.First();
            Reset();
        }

        #region IEnumerator<T> Members

        public T Current
        {
            get
            {
                if (_current == null || _finished) throw new InvalidOperationException();
                return _current;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get
            {
                if (_current == null || _finished) throw new InvalidOperationException();
                return _current;
            }
        }

        public bool MoveNext()
        {
            if (!_finished)
            {
                _current = _current == null ? _first : _current.Next;
                _finished = _current == null;
            }
            return !_finished;
        }

        public void Reset()
        {
            _finished = false;
            _current = null;
        }

        #endregion
    }
}
