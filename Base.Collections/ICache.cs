using System;
using System.Collections.Generic;

namespace Base.Collections
{
    public interface ICache<K, V>
    {
        /// <summary>
        /// Lists keys that this cache holds.
        /// Keys are never weak, so this is reliable
        /// The operation is thread-safe because it returns a copy of the list of keys in the underlying dictionary.
        /// </summary>
        IReadOnlyList<K> Keys { get; }

        /// <summary>
        /// Gets cached value corresponding to the given key. If the value is not in the cache 
        /// (or was collected in the case of a weak cache),  getter function is called. 
        /// If key is null, no cache entry is created and default(V) is returned unconditionally.
        /// The operation is thread-safe.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        V Get(K key, Func<K, V> factory);

        /// <summary>
        /// If cache contains value corresponding to the given key, the value is returned.
        /// Otherwise, default(V) is returned.
        /// The operation is thread-safe.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        V Peek(K key);

        /// <summary>
        /// Clears the cache.
        /// The operation is thread-safe.
        /// </summary>
        void Clear();

        /// <summary>
        /// Removes cache entry for the given key, if it exists, otherwise does nothing.
        /// The operation is thread-safe.
        /// </summary>
        /// <param name="key"></param>
        void Invalidate(K key);
    }
}