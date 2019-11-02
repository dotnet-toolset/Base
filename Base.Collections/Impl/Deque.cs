using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Base.Collections.Impl
{
    /// <summary>
    /// Represents a simple double-ended-queue collection of objects.
    /// </summary>
    public class Deque<T> : IDeque<T>
    {
        #region Deque Members

        #region Fields

        // The node at the front of the deque.
        private Node _front;

        // The node at the back of the deque.
        private Node _back;

        // The number of elements in the deque.
        private int _count;

        // The version of the deque.
        private long _version;

        #endregion

        public T this[int index] => this.ElementAt(index);

        #region Construction

        /// <summary>
        /// Initializes a new instance of the Deque class.
        /// </summary>
        public Deque()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Deque class that contains 
        /// elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements are copied to the new Deque.
        /// </param>
        public Deque(IEnumerable<T> collection)
        {
            #region Require

            if (collection == null)
                throw new ArgumentNullException("col");

            #endregion

            foreach (var item in collection)
                PushBack(item);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Removes all objects from the Deque.
        /// </summary>
        public virtual void Clear()
        {
            _count = 0;
            _front = _back = null;
            _version++;
            AssertValid();
        }

        /// <summary>
        /// Determines whether or not an element is in the Deque.
        /// </summary>
        /// <param name="obj">
        /// The Object to locate in the Deque.
        /// </param>
        /// <returns>
        /// <b>true</b> if <i>obj</i> if found in the Deque; otherwise, 
        /// <b>false</b>.
        /// </returns>
        public virtual bool Contains(T obj)
        {
            foreach (var o in this)
            {
                if (EqualityComparer<T>.Default.Equals(o, obj))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inserts an object at the front of the Deque.
        /// </summary>
        /// <param name="item">
        /// The object to push onto the deque;
        /// </param>
        public virtual void PushFront(T item)
        {
            // The new node to add to the front of the deque.
            var newNode = new Node(item) {Next = _front};

            // Link the new node to the front node. The current front node at 
            // the front of the deque is now the second node in the deque.

            // If the deque isn't empty.
            if (Count > 0)
            {
                // Link the current front to the new node.
                _front.Previous = newNode;
            }

            // Make the new node the front of the deque.
            _front = newNode;

            // Keep track of the number of elements in the deque.
            _count++;

            // If this is the first element in the deque.
            if (Count == 1)
            {
                // The front and back nodes are the same.
                _back = _front;
            }

            _version++;

            #region Invariant

            AssertValid();

            #endregion
        }

        /// <summary>
        /// Inserts an object at the back of the Deque.
        /// </summary>
        /// <param name="item">
        /// The object to push onto the deque;
        /// </param>
        public virtual void PushBack(T item)
        {
            // The new node to add to the back of the deque.
            var newNode = new Node(item) {Previous = _back};

            // Link the new node to the back node. The current back node at 
            // the back of the deque is now the second to the last node in the
            // deque.

            // If the deque is not empty.
            if (Count > 0)
            {
                // Link the current back node to the new node.
                _back.Next = newNode;
            }

            // Make the new node the back of the deque.
            _back = newNode;

            // Keep track of the number of elements in the deque.
            _count++;

            // If this is the first element in the deque.
            if (Count == 1)
            {
                // The front and back nodes are the same.
                _front = _back;
            }

            _version++;

            #region Invariant

            AssertValid();

            #endregion
        }

        /// <summary>
        /// Removes and returns the object at the front of the Deque.
        /// </summary>
        /// <returns>
        /// The object at the front of the Deque.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The Deque is empty.
        /// </exception>
        public virtual T PopFront()
        {
            #region Require

            if (Count == 0)
            {
                throw new InvalidOperationException("Deque is empty.");
            }

            #endregion

            // Get the object at the front of the deque.
            var item = _front.Value;

            // Move the front back one node.
            _front = _front.Next;

            // Keep track of the number of nodes in the deque.
            _count--;

            // If the deque is not empty.
            if (Count > 0)
            {
                // Tie off the previous link in the front node.
                _front.Previous = null;
            }
            // Else the deque is empty.
            else
            {
                // Indicate that there is no back node.
                _back = null;
            }

            _version++;

            #region Invariant

            AssertValid();

            #endregion

            return item;
        }

        /// <summary>
        /// Removes and returns the object at the back of the Deque.
        /// </summary>
        /// <returns>
        /// The object at the back of the Deque.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The Deque is empty.
        /// </exception>
        public virtual T PopBack()
        {
            #region Require

            if (Count == 0)
            {
                throw new InvalidOperationException("Deque is empty.");
            }

            #endregion

            // Get the object at the back of the deque.
            var item = _back.Value;

            // Move back node forward one node.
            _back = _back.Previous;

            // Keep track of the number of nodes in the deque.
            _count--;

            // If the deque is not empty.
            if (Count > 0)
            {
                // Tie off the next link in the back node.
                _back.Next = null;
            }
            // Else the deque is empty.
            else
            {
                // Indicate that there is no front node.
                _front = null;
            }

            _version++;

            #region Invariant

            AssertValid();

            #endregion

            return item;
        }

        /// <summary>
        /// Returns the object at the front of the Deque without removing it.
        /// </summary>
        /// <returns>
        /// The object at the front of the Deque.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The Deque is empty.
        /// </exception>
        public virtual T PeekFront()
        {
            #region Require

            if (Count == 0)
            {
                throw new InvalidOperationException("Deque is empty.");
            }

            #endregion

            return _front.Value;
        }

        /// <summary>
        /// Returns the object at the back of the Deque without removing it.
        /// </summary>
        /// <returns>
        /// The object at the back of the Deque.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The Deque is empty.
        /// </exception>
        public virtual T PeekBack()
        {
            #region Require

            if (Count == 0)
            {
                throw new InvalidOperationException("Deque is empty.");
            }

            #endregion

            return _back.Value;
        }

        /// <summary>
        /// Copies the Deque to a new array.
        /// </summary>
        /// <returns>
        /// A new array containing copies of the elements of the Deque.
        /// </returns>
        public virtual T[] ToArray()
        {
            var array = new T[Count];
            var index = 0;

            foreach (var item in this)
            {
                array[index] = item;
                index++;
            }

            return array;
        }

        /// <summary>
        /// Returns a synchronized (thread-safe) wrapper for the Deque.
        /// </summary>
        /// <param name="deque">
        /// The Deque to synchronize.
        /// </param>
        /// <returns>
        /// A synchronized wrapper around the Deque.
        /// </returns>
        public static Deque<T> Synchronized(Deque<T> deque)
        {
            #region Require

            if (deque == null)
            {
                throw new ArgumentNullException(nameof(deque));
            }

            #endregion

            return new SynchronizedDeque(deque);
        }

        [Conditional("DEBUG")]
        private void AssertValid()
        {
            var n = 0;
            var current = _front;

            while (current != null)
            {
                n++;
                current = current.Next;
            }

            Debug.Assert(n == Count);

            if (Count > 0)
            {
                Debug.Assert(_front != null && _back != null, "Front/Back Null Test - Count > 0");

                var f = _front;
                var b = _back;

                while (f.Next != null && b.Previous != null)
                {
                    f = f.Next;
                    b = b.Previous;
                }

                Debug.Assert(f.Next == null && b.Previous == null, "Front/Back Termination Test");
                Debug.Assert(f == _back && b == _front, "Front/Back Equality Test");
            }
            else
            {
                Debug.Assert(_front == null && _back == null, "Front/Back Null Test - Count == 0");
            }
        }

        #endregion       

        #endregion

        #region ICollection Members

        /// <summary>
        /// Gets a value indicating whether access to the Deque is synchronized 
        /// (thread-safe).
        /// </summary>
        public virtual bool IsSynchronized => false;

        /// <summary>
        /// Gets the number of elements contained in the Deque.
        /// </summary>
        public virtual int Count => _count;

        /// <summary>
        /// Copies the Deque elements to an existing one-dimensional Array, 
        /// starting at the specified array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional Array that is the destination of the elements 
        /// copied from Deque. The Array must have zero-based indexing. 
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins. 
        /// </param>
        public virtual void CopyTo(Array array, int index)
        {
            #region Require

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    "Index is less than zero.");
            }

            if (array.Rank > 1)
            {
                throw new ArgumentException("Array is multidimensional.");
            }

            if (index >= array.Length)
            {
                throw new ArgumentException("Index is equal to or greater " +
                                            "than the length of array.");
            }

            if (Count > array.Length - index)
            {
                throw new ArgumentException(
                    "The number of elements in the source Deque is greater " +
                    "than the available space from index to the end of the " +
                    "destination array.");
            }

            #endregion

            var i = index;

            foreach (object obj in this)
            {
                array.SetValue(obj, i);
                i++;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the Deque.
        /// </summary>
        public virtual object SyncRoot => this;

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that can iterate through the Deque.
        /// </summary>
        /// <returns>
        /// An IEnumerator for the Deque.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a shallow copy of the Deque.
        /// </summary>
        /// <returns>
        /// A shallow copy of the Deque.
        /// </returns>
        public virtual object Clone()
        {
            var clone = new Deque<T>(this) {_version = _version};
            return clone;
        }

        #endregion

        #region IEnumerable<T> Members

        public virtual IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region Enumerator Class

        [Serializable()]
        private class Enumerator : IEnumerator<T>
        {
            private readonly Deque<T> _owner;

            private Node _currentNode;

            private T _current;

            private bool _moveResult;

            private readonly long _version;

            // A value indicating whether the enumerator has been disposed.
            private bool _disposed;

            public Enumerator(Deque<T> owner)
            {
                _owner = owner;
                _currentNode = owner._front;
                _version = owner._version;
            }

            #region IEnumerator Members

            public void Reset()
            {
                #region Require

                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                if (_version != _owner._version)
                {
                    throw new InvalidOperationException(
                        "The Deque was modified after the enumerator was created.");
                }

                #endregion

                _currentNode = _owner._front;
                _moveResult = false;
            }

            public object Current
            {
                get
                {
                    #region Require

                    if (_disposed)
                    {
                        throw new ObjectDisposedException(GetType().Name);
                    }

                    if (!_moveResult)
                    {
                        throw new InvalidOperationException(
                            "The enumerator is positioned before the first " +
                            "element of the Deque or after the last element.");
                    }

                    #endregion

                    return _current;
                }
            }

            public bool MoveNext()
            {
                #region Require

                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                if (_version != _owner._version)
                {
                    throw new InvalidOperationException(
                        "The Deque was modified after the enumerator was created.");
                }

                #endregion

                if (_currentNode != null)
                {
                    _current = _currentNode.Value;
                    _currentNode = _currentNode.Next;

                    _moveResult = true;
                }
                else
                {
                    _moveResult = false;
                }

                return _moveResult;
            }

            #endregion

            #region IEnumerator<T> Members

            T IEnumerator<T>.Current
            {
                get
                {
                    #region Require

                    if (_disposed)
                    {
                        throw new ObjectDisposedException(GetType().Name);
                    }

                    if (!_moveResult)
                    {
                        throw new InvalidOperationException(
                            "The enumerator is positioned before the first " +
                            "element of the Deque or after the last element.");
                    }

                    #endregion

                    return _current;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                _disposed = true;
            }

            #endregion
        }

        #endregion

        #region Node Class

        // Represents a node in the deque.
        private class Node
        {
            public readonly T Value;
            public Node Previous;
            public Node Next;

            public Node(T value)
            {
                Value = value;
            }
        }

        #endregion

        #region SynchronizedDeque Class

        // Implements a synchronization wrapper around a deque.
        [Serializable()]
        private class SynchronizedDeque : Deque<T>, IEnumerable
        {
            #region SynchronziedDeque Members

            #region Fields

            // The wrapped deque.
            private readonly Deque<T> _deque;

            // The object to lock on.
            private readonly object _root;

            #endregion

            #region Construction

            public SynchronizedDeque(Deque<T> deque)
            {

                _deque = deque ?? throw new ArgumentNullException(nameof(deque));
                _root = deque.SyncRoot;
            }

            #endregion

            #region Methods

            public override void Clear()
            {
                lock (_root)
                {
                    _deque.Clear();
                }
            }

            public override bool Contains(T item)
            {
                lock (_root)
                {
                    return _deque.Contains(item);
                }
            }

            public override void PushFront(T item)
            {
                lock (_root)
                {
                    _deque.PushFront(item);
                }
            }

            public override void PushBack(T item)
            {
                lock (_root)
                {
                    _deque.PushBack(item);
                }
            }

            public override T PopFront()
            {
                lock (_root)
                {
                    return _deque.PopFront();
                }
            }

            public override T PopBack()
            {
                lock (_root)
                {
                    return _deque.PopBack();
                }
            }

            public override T PeekFront()
            {
                lock (_root)
                {
                    return _deque.PeekFront();
                }
            }

            public override T PeekBack()
            {
                lock (_root)
                {
                    return _deque.PeekBack();
                }
            }

            public override T[] ToArray()
            {
                lock (_root)
                {
                    return _deque.ToArray();
                }
            }

            public override object Clone()
            {
                lock (_root)
                {
                    return _deque.Clone();
                }
            }

            public override void CopyTo(Array array, int index)
            {
                lock (_root)
                {
                    _deque.CopyTo(array, index);
                }
            }

            public override IEnumerator<T> GetEnumerator()
            {
                lock (_root)
                {
                    return _deque.GetEnumerator();
                }
            }

            /// <summary>
            /// Returns an enumerator that can iterate through the Deque.
            /// </summary>
            /// <returns>
            /// An IEnumerator for the Deque.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                lock (_root)
                {
                    return ((IEnumerable)_deque).GetEnumerator();
                }
            }

            #endregion

            #region Properties

            public override int Count
            {
                get
                {
                    lock (_root)
                    {
                        return _deque.Count;
                    }
                }
            }

            public override bool IsSynchronized => true;

            #endregion

            #endregion
        }

        #endregion	

    }
}
