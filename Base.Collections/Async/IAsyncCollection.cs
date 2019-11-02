﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Collections.Async
{
    /// <summary>
    /// Represents a thread-safe collection that allows asynchronous consuming.
    /// </summary>
    /// <typeparam name="T">The type of the items contained in the collection.</typeparam>
    public interface IAsyncCollection<T> : IEnumerable<T>, System.Collections.ICollection
    {
        /// <summary>
        /// Gets an amount of pending item requests.
        /// </summary>
        int AwaiterCount { get; }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        void Add(T item);
        void CompleteAdding();

        /// <summary>
        /// Removes and returns an item from the collection in an asynchronous manner.
        /// </summary>
        Task<TakeResult<T>> TakeAsync(CancellationToken ct);
        TakeResult<T> Take();

    }
}
