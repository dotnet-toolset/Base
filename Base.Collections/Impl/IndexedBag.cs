using System.Collections.Generic;
using System.Linq;

namespace Base.Collections.Impl
{
    public class IndexedBag<T> : IIndexedBag<T>
    {
        private readonly Dictionary<T, int> _map;

        public int Count => _map.Count;

        public IndexedBag()
        {
            _map = new Dictionary<T, int>();
        }

        public IndexedBag(IEqualityComparer<T> comparer)
        {
            _map = new Dictionary<T, int>(comparer);
        }

        public int Put(T item)
        {
            int index;
            lock (_map)
                if (!_map.TryGetValue(item, out index))
                    _map.Add(item, index = _map.Count);
            return index;
        }

        public int Get(T item)
        {
            lock (_map)
                return _map.TryGetValue(item, out var index) ? index : -1;
        }

        public IList<T> ToList()
        {
            lock (_map)
                return _map.OrderBy(e => e.Value).Select(e => e.Key).ToList();
        }

        public IList<T> ToList(int start)
        {
            lock (_map)
                return _map.Where(e => e.Value >= start).OrderBy(e => e.Value).Select(e => e.Key).ToList();
        }

        public T[] ToArray()
        {
            lock (_map)
                return _map.OrderBy(e => e.Value).Select(e => e.Key).ToArray();
        }

        public T[] ToArray(int start)
        {
            lock (_map)
                return _map.Where(e => e.Value >= start).OrderBy(e => e.Value).Select(e => e.Key).ToArray();
        }
    }
}
