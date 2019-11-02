using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Base.Collections.Impl
{
    public class ReadOnlySet<T> : IReadOnlySet<T>
    {
        public static ReadOnlySet<T> Empty = new ReadOnlySet<T>();

        private readonly ISet<T> _base;

        public bool IsEmpty => _base.Count == 0;

        private ReadOnlySet()
        {
            _base = new HashSet<T>();
        }

        public ReadOnlySet(params T[] items)
        {
            _base = new HashSet<T>(items);
        }

        public ReadOnlySet(IEnumerable<T> items)
        {
            _base = new HashSet<T>(items);
        }

        public ReadOnlySet(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            _base = new HashSet<T>(items, comparer);
        }

        /// <summary>
        /// Initializes with a reference to the given set as an underlying storage.
        /// If the provided set changes, the readonly set follows the change
        /// </summary>
        /// <param name="items"></param>
        public ReadOnlySet(ISet<T> items)
        {
            _base = items;
        }

        int IReadOnlyCollection<T>.Count => _base.Count;

        public bool Contains(T item) => _base.Contains(item);

        public IEnumerator<T> GetEnumerator() => _base.GetEnumerator();

        public bool IsProperSubsetOf(IEnumerable<T> other) => _base.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => _base.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => _base.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => _base.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => _base.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) => _base.SetEquals(other);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();


        public class ThreadSafe : IReadOnlySet<T>
        {
            private readonly ISet<T> _base;

            public bool IsEmpty => _base.Count == 0;

            private ThreadSafe()
            {
                _base = new HashSet<T>();
            }

            public ThreadSafe(params T[] items)
            {
                _base = new HashSet<T>(items);
            }

            public ThreadSafe(IEnumerable<T> items)
            {
                _base = new HashSet<T>(items);
            }

            public ThreadSafe(IEnumerable<T> items, IEqualityComparer<T> comparer)
            {
                _base = new HashSet<T>(items, comparer);
            }

            /// <summary>
            /// Initializes with a reference to the given set as an underlying storage.
            /// If the provided set changes, the readonly set follows the change
            /// </summary>
            /// <param name="items"></param>
            public ThreadSafe(ISet<T> items)
            {
                _base = items;
            }

            int IReadOnlyCollection<T>.Count
            {
                get
                {
                    lock (_base) return _base.Count;
                }
            }

            public bool Contains(T item)
            {
                lock (_base)
                    return _base.Contains(item);
            }

            public IEnumerator<T> GetEnumerator()
            {
                lock (_base)
                    return _base.ToList().GetEnumerator();
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                lock (_base)
                    return _base.IsProperSubsetOf(other);
            }

            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                lock (_base)
                    return _base.IsProperSupersetOf(other);
            }

            public bool IsSubsetOf(IEnumerable<T> other)
            {
                lock (_base)
                    return _base.IsSubsetOf(other);
            }

            public bool IsSupersetOf(IEnumerable<T> other)
            {
                lock (_base)
                    return _base.IsSupersetOf(other);
            }

            public bool Overlaps(IEnumerable<T> other)
            {
                lock (_base)
                    return _base.Overlaps(other);
            }

            public bool SetEquals(IEnumerable<T> other)
            {
                lock (_base)
                    return _base.SetEquals(other);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }

    public class ReadOnlySortedSet<T> : ReadOnlySet<T>, IReadOnlySortedSet<T>
    {
        private readonly SortedSet<T> _set;

        public T Max => _set.Max;
        public T Min => _set.Min;
        public IComparer<T> Comparer => _set.Comparer;

        public ReadOnlySortedSet(SortedSet<T> items)
            : base(items)
        {
            _set = items;
        }


        public IReadOnlySortedSet<T> GetViewBetween(T lowerValue, T upperValue) =>
            new ReadOnlySortedSet<T>(_set.GetViewBetween(lowerValue, upperValue));

        public bool TryGetValue(T equalValue, out T actualValue)
        {
#if NET461 || NETSTANDARD
            throw new NotImplementedException();
#else
            return _set.TryGetValue(equalValue, out actualValue);
#endif
        }

        public new class ThreadSafe : ReadOnlySet<T>.ThreadSafe, IReadOnlySortedSet<T>
        {
            private readonly SortedSet<T> _set;

            public T Max
            {
                get
                {
                    lock (_set) return _set.Max;
                }
            }

            public T Min
            {
                get
                {
                    lock (_set) return _set.Min;
                }
            }

            public IComparer<T> Comparer => _set.Comparer;

            public ThreadSafe(SortedSet<T> items)
                : base(items)
            {
                _set = items;
            }


            public IReadOnlySortedSet<T> GetViewBetween(T lowerValue, T upperValue)
            {
                lock (_set)
                    return new ReadOnlySortedSet<T>(new SortedSet<T>(_set.GetViewBetween(lowerValue, upperValue)));
            }

            public bool TryGetValue(T equalValue, out T actualValue)
            {
#if NET461 || NETSTANDARD
                throw new NotImplementedException();
#else
                lock (_set)
                    return _set.TryGetValue(equalValue, out actualValue);
#endif
            }
        }
    }
}