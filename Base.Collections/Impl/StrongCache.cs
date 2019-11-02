using System;
using System.Collections.Generic;
using System.Linq;

namespace Base.Collections.Impl
{
    public class StrongCache<K, V> : ICache<K, V>
    {
        private readonly Dictionary<K, V> _dictionary;

        public IReadOnlyList<K> Keys { get { lock (_dictionary) return _dictionary.Keys.ToList(); } }

        public StrongCache()
        {
            _dictionary = new Dictionary<K, V>();
        }

        public StrongCache(IEqualityComparer<K> comparer) { _dictionary = new Dictionary<K, V>(comparer); }

        public V Get(K aKey, Func<K, V> factory)
        {
            var vResult = default(V);
            if (!Equals(aKey, null))
                lock (_dictionary)
                {
                    if (!_dictionary.TryGetValue(aKey, out vResult) && factory != null)
                        _dictionary[aKey] = vResult = factory(aKey);
                }
            return vResult;
        }

        public V Peek(K aKey)
        {
            var vResult = default(V);
            if (!Equals(aKey, null))
                lock (_dictionary)
                    _dictionary.TryGetValue(aKey, out vResult);
            return vResult;
        }

        public void Clear()
        {
            lock (_dictionary)
                _dictionary.Clear();
        }

        public void Invalidate(K aKey)
        {
            if (!Equals(aKey, null))
                lock (this)
                    _dictionary?.Remove(aKey);
        }

    }
}
