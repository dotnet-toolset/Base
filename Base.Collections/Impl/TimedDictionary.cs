using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Base.Collections.Impl
{
    public class TimedDictionary<TK, TV> : IDictionary<TK, TV>
    {
        private readonly Stopwatch _stopwatch;
        private readonly TimeSpan _lifetime;
        private readonly ConcurrentDictionary<TK, Tuple<TV, TimeSpan>> _map;

        public TimedDictionary(TimeSpan lifetime)
        {
            _lifetime = lifetime;
            _map = new ConcurrentDictionary<TK, Tuple<TV, TimeSpan>>();
            _stopwatch = Stopwatch.StartNew();
        }

        public TimeSpan? Remaining(TK key)
        {
            if (!_map.TryGetValue(key, out var value)) return null;
            var now = _stopwatch.Elapsed;
            if (value.Item2 <= now)
            {
                if (_map.TryRemove(key, out var removedValue) && !ReferenceEquals(value, removedValue) && removedValue.Item2 > now)
                    value = _map[key] = removedValue;
                else return null;
            }
            return value.Item2 - now;
        }


        public void Add(KeyValuePair<TK, TV> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TK key, TV value, TimeSpan lifetime)
        {
            if (Remaining(key).HasValue || !_map.TryAdd(key, new Tuple<TV, TimeSpan>(value, _stopwatch.Elapsed + lifetime)))
                throw new Exception("duplicate key " + key);
        }

        public void Clear()
        {
            _map.Clear();
        }

        public bool Contains(KeyValuePair<TK, TV> item)
        {
            return Remaining(item.Key).HasValue;
        }

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            return _map.TryRemove(item.Key, out _);
        }

        public int Count => _map.Count;

        public bool IsReadOnly => false;

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return _map.Where(kv => Remaining(kv.Key).HasValue).Select(kv => new KeyValuePair<TK, TV>(kv.Key, kv.Value.Item1)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(TK key)
        {
            return Remaining(key).HasValue;
        }

        public void Add(TK key, TV value)
        {
            Add(key, value, _lifetime);
        }

        public bool Remove(TK key)
        {
            return _map.TryRemove(key, out _);
        }

        public bool TryGetValue(TK key, out TV value)
        {
            if (_map.TryGetValue(key, out var v))
            {
                value = v.Item1;
                return Remaining(key).HasValue;
            }
            value = default(TV);
            return false;
        }

        public TV this[TK key]
        {
            get => TryGetValue(key, out var value) ? value : default(TV);
            set => _map[key] = new Tuple<TV, TimeSpan>(value, _stopwatch.Elapsed + _lifetime);
        }

        public ICollection<TK> Keys
        {
            get { return _map.Keys.Where(k => Remaining(k).HasValue).ToList(); }
        }

        public ICollection<TV> Values
        {
            get { return _map.Where(kv => Remaining(kv.Key).HasValue).Select(kv => kv.Value.Item1).ToList(); }
        }
    }
}
