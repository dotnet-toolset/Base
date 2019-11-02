using System;
using System.Collections.Generic;

namespace Base.Collections.Impl
{
    public class AbstractNode<T> : INode<T> where T : class, INode<T>
    {
        private AbstractNode<T> _parent, _next, _prev, _head;

        #region INode<T> Members

        public T ParentNode => (T)(object)_parent;
        public T FirstChild => (T)(object)_head;
        public T LastChild => (T)(object)_head?._prev;
        public T PreviousSibling => (T)(object)(_prev != null && (_parent == null || !ReferenceEquals(this, _parent._head)) ? _prev : null);
        public T NextSibling => (T)(object)(_next != null && (_parent == null || !ReferenceEquals(_next, _parent._head)) ? _next : null);

        public virtual void Insert(T node, T before = null)
        {
            var n = node as AbstractNode<T>;
            if (n == null) return;
            if (n._parent != null) throw new ArgumentException("this node is already a tree member");
            n._parent = this;
            if (_head == null)
            {
                n._next = n;
                n._prev = n;
                _head = n;
            }
            else
            {
                var child = before as AbstractNode<T>;
                var next = child ?? _head;
                n._next = next;
                n._prev = next._prev;
                next._prev._next = n;
                next._prev = n;
                if (child != null && ReferenceEquals(child, _head))
                    _head = n;
            }
        }

        public virtual bool Remove(T child)
        {
            var node = child as AbstractNode<T>;
            if (node == null || node._parent != this) return false;
            if (ReferenceEquals(node._next, node))
                _head = null;
            else
            {
                node._next._prev = node._prev;
                node._prev._next = node._next;
                if (_head == node)
                    _head = node._next;
            }
            node._parent = null;
            node._prev = null;
            node._next = null;
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new ChildrenEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ChildrenEnumerator(this);
        }

        #endregion

        private class ChildrenEnumerator : IEnumerator<T>
        {
            private readonly AbstractNode<T> _node;
            private bool _finished;
            private T _current;

            internal ChildrenEnumerator(AbstractNode<T> aNode)
            {
                _node = aNode;
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
                    _current = _current == null ? _node.FirstChild : _current.NextSibling;
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
}
