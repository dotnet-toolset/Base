using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.Collections.Impl
{
    public class WeakCache<K, V> : ICache<K, V>
    {
        private static readonly K[] EmptyKeys = new K[0];
        private readonly IEqualityComparer<K> _comparer;
        private volatile Dictionary<K, WeakReference> _dictionary;

        public IReadOnlyList<K> Keys
        {
            get
            {
                var dictionary = _dictionary;
                if (dictionary == null) return EmptyKeys;
                lock (dictionary) return dictionary.Keys.ToList();
            }
        }

        public WeakCache()
        {
        }

        public WeakCache(IEqualityComparer<K> comparer)
        {
            _comparer = comparer;
        }

        private static bool IsNullValue(object a) => Equals(a, default(V));

        public V Get(K key, Func<K, V> factory)
        {
            var result = default(V);
            if (!ReferenceEquals(key, null))
            {
                var dictionary = _dictionary;
                if (dictionary == null)
                    lock (this)
                    {
                        dictionary = _dictionary;
                        if (dictionary == null && factory != null)
                            dictionary = _dictionary = new Dictionary<K, WeakReference>(_comparer);
                    }

                if (dictionary != null)
                    lock (dictionary)
                    {
                        if (dictionary.TryGetValue(key, out var vReference) && vReference != null)
                        {
                            result = (V) vReference.Target;
                            if (IsNullValue(result)) dictionary.Remove(key);
                        }

                        if (IsNullValue(result) && factory != null)
                            dictionary[key] = new WeakReference(result = factory(key));
                    }
            }

            return result;
        }

        public V Peek(K key)
        {
            var result = default(V);
            if (!Equals(key, null))
            {
                var dictionary = _dictionary;
                if (dictionary != null)
                    lock (dictionary)
                        if (dictionary.TryGetValue(key, out var vReference))
                        {
                            result = (V) vReference.Target;
                            if (IsNullValue(result)) dictionary.Remove(key);
                        }
            }

            return result;
        }

        public void Clear()
        {
            lock (this)
                if (_dictionary != null)
                {
                    lock (_dictionary)
                        _dictionary.Clear();
                    _dictionary = null;
                }
        }

        public void Invalidate(K aKey)
        {
            if (ReferenceEquals(aKey, null)) return;
            var dictionary = _dictionary;
            if (dictionary != null)
                lock (dictionary)
                    dictionary.Remove(aKey);
        }
    }
}