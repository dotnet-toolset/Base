using System;
using System.Collections;
using System.Collections.Generic;

namespace Base.Collections.Impl
{
    /// <summary>
    /// A set that maintains insert order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrderedSet<T> : ISet<T>, IList<T>, IList, IReadOnlySet<T>
    {
        private readonly List<T> _list;
        private readonly HashSet<T> _set;

        public bool IsEmpty => _list.Count==0;

        /// <summary>
        /// Need to have parameterless constructor for automatic instance creation
        /// </summary>
        public OrderedSet()
        {
            _list = new List<T>();
            _set = new HashSet<T>();
        }

        public OrderedSet(IEnumerable<T> list)
            : this()
        {
            AddRange(list);
        }

        public OrderedSet(IEqualityComparer<T> aComparer)
        {
            _list = new List<T>();
            _set = new HashSet<T>(aComparer);
        }

        #region ISet<T> Members

        public bool Add(T item)
        {
            var vResult = _set.Add(item);
            if (vResult) _list.Add(item);
            return vResult;
        }

        private void Set2List()
        {
            var vToAdd = new HashSet<T>(_set);
            foreach (var i in _list)
                if (!_set.Contains(i)) _list.Remove(i);
                else vToAdd.Remove(i);
            _list.AddRange(vToAdd);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _set.ExceptWith(other);
            Set2List();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _set.IntersectWith(other);
            Set2List();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _set.SymmetricExceptWith(other);
            Set2List();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _set.UnionWith(other);
            Set2List();
        }

        #endregion

        #region ICollection<T> Members

        void ICollection<T>.Add(T item)
        {
            var vResult = _set.Add(item);
            if (vResult) _list.Add(item);
        }

        public void Clear()
        {
            _set.Clear();
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public bool Remove(T item)
        {
            _set.Remove(item);
            return _list.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        public void AddRange(IEnumerable<T> aEnumerable)
        {
            foreach (var i in aEnumerable)
                Add(i);
        }


        public void InsertBefore(T beforeChild, T newChild)
        {
            var i = _list.IndexOf(beforeChild);
            if (i == -1) Add(newChild); else Insert(i, newChild);
        }

        public void Sort()
        {
            _list.Sort();
        }

        public void Sort(Comparison<T> comparison)
        {
            _list.Sort(comparison);
        }

        public void Sort(IComparer<T> comparer)
        {
            _list.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _list.Sort(index, count, comparer);
        }
        
        /// <summary>
        /// Add item to the set and return its index.
        /// If the item already exists, the index of existing item is returned
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int Put(T item)
        {
            int vResult;
            var vAdded = _set.Add(item);
            if (vAdded)
            {
                _list.Add(item);
                vResult = _list.Count - 1;
            }
            else vResult = _list.IndexOf(item);
            return vResult;
        }

        #region IList Members

        object IList.this[int i]
        {
            get => _list[i];
            set
            {
                if (_set.Contains((T)value) && IndexOf(value) != i) throw new ArgumentException("duplicate in ordered set");
                _list[i] = (T)value; _set.Add((T)value);
            }
        }
        public T this[int i]
        {
            get => _list[i];
            set
            {
                if (_set.Contains(value) && IndexOf(value) != i) throw new ArgumentException("duplicate in ordered set");
                _list[i] = value; _set.Add(value);
            }
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            var vResult = _set.Add(item);
            if (vResult) _list.Insert(index, item);
        }

        public int Add(object item)
        {
            int vResult = -1;
            var vAdded = _set.Add((T)item);
            if (vAdded)
            {
                _list.Add((T)item);
                vResult = _list.Count - 1;
            }
            return vResult;
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public bool IsFixedSize => false;

        public void Remove(object value)
        {
            Remove((T)value);
        }

        public void RemoveAt(int aIndex)
        {
            var i = _list[aIndex];
            _list.RemoveAt(aIndex);
            _set.Remove(i);
        }


        #endregion

        #region ICollection Members

        public bool IsSynchronized => false;

        public object SyncRoot => _list;


        public void CopyTo(Array array, int index)
        {
            _list.CopyTo((T[])array, index);
        }

        #endregion
    }
}
