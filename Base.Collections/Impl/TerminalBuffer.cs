using System.Collections.Generic;

namespace Base.Collections.Impl
{
    /// <summary>
    /// Collection that holds limited number of items, auto-removes older items when new items are added. 
    /// </summary>
    /// <typeparam name="T">type of items</typeparam>
    public class TerminalBuffer<T> : ICollection<T>
    {
        private readonly LinkedList<T> _list;
        private int _limit;

        /// <summary>
        /// Maximum number of items in this collection, may be changed anytime
        /// </summary>
        public int Limit
        {
            get => _limit;
            set
            {
                if (_limit == value) return;
                _limit = value; lock (this) CheckLimit();
            }
        }

        /// <summary>
        /// Create new instance with the default limit of 100 items
        /// </summary>
        public TerminalBuffer() // need to keep this so we can auto-instantiate this object
            : this(100)
        {
        }

        /// <summary>
        /// Create new instance with the provided limit
        /// </summary>
        /// <param name="limit">maximum number of items</param>
        public TerminalBuffer(int limit)
        {
            _limit = limit;
            _list = new LinkedList<T>();
        }

        private void CheckLimit()
        {
            var vLimit = _limit;
            var c = _list.Count - vLimit;
            while (c-- > 0)
                _list.RemoveFirst();
        }

        /// <summary>
        /// Adds multiple items to the collection at once
        /// </summary>
        /// <param name="items">items to add</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items != null)
                foreach (var i in items)
                    Add(i);
        }

        #region ICollection<T> Members

        /// <inheritdoc />
        public void Add(T item)
        {
            lock (this)
            {
                _list.AddLast(item);
                CheckLimit();
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            lock (this)
                _list.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            lock (this)
                return _list.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this)
                _list.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public int Count
        {
            get { lock (this) return _list.Count; }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool Remove(T item)
        {
            lock (this)
                return _list.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            lock (this)
                return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (this)
                return _list.GetEnumerator();
        }

        #endregion
    }
}
