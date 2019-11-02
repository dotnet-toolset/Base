using System;
using System.Collections.Generic;

namespace Base.Collections.Impl
{
    public class SortedLinkedList<T> : ISortedLinkedList<T>
    {
        public class Node:ISortedLinkedListNode<T>
        {
            public  T Value { get; }
            internal Node Next;

            internal Node(T value)
            {
                Value = value;
            }
        }

        private readonly Comparison<T> _comparison;

        private Node _head;
        private int count;

        public ISortedLinkedListNode<T> Head => _head;

        public SortedLinkedList()
        {
            var t = typeof(T);
            if (typeof(IComparable<T>).IsAssignableFrom(t))
                _comparison = GenericCompare;
            else if (typeof(IComparable).IsAssignableFrom(t))
                _comparison = ObjectCompare;
            else throw new NotSupportedException("either use constructor with comparer or provide comparable type");
        }

        private static int GenericCompare(T a, T b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a != null) return ((IComparable<T>)a).CompareTo(b);
            if (b != null) return -((IComparable<T>)b).CompareTo(a);
            throw new NotSupportedException();
        }

        private static int ObjectCompare(T a, T b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a != null) return ((IComparable)a).CompareTo(b);
            if (b != null) return -((IComparable)b).CompareTo(a);
            throw new NotSupportedException();
        }

        public SortedLinkedList(IComparer<T> comparer)
        {
            _comparison = comparer.Compare;
        }

        public SortedLinkedList(Comparison<T> comparison)
        {
            _comparison = comparison;
        }

        public ISortedLinkedListNode<T> Insert(T value) // inserts in order
        {
            Node prev = null;
            var curr = _head;
            while (curr != null && _comparison(curr.Value, value) < 0)
            {
                prev = curr;
                curr = curr.Next;
            }
            var n = new Node(value);
            if (prev == null) // inserting at beginning of list
            {
                n.Next = _head;
                _head = n;
            }
            else // inserting in middle or at end
            {
                prev.Next = n;
                n.Next = curr;
            }
            ++count;
            return n;
        }

        public bool Contains(T value)
        {
            return Find(value) != null;
        }

        public ISortedLinkedListNode<T> Find(T value)
        {
            Node prev = null;
            Node curr = _head;
            while (curr != null && _comparison(curr.Value, value) != 0)
            {
                prev = curr;
                curr = curr.Next;
            }
            return curr;
        }

        public ISortedLinkedListNode<T> Delete(T value)
        {
            Node prev = null;
            Node curr = _head;
            while (curr != null && _comparison(curr.Value, value) != 0)
            {
                prev = curr;
                curr = curr.Next;
            }
            if (prev == null) // deleting first node
            {
                if (curr == null) return null;
                _head = curr.Next;
            }
            else // deleting any node other than first node
                prev.Next = curr.Next;
            --count;
            return curr;
        }
    }
}
