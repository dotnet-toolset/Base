using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Base.Collections.Impl.Async
{
    /// <summary>
    /// Represents a thread-safe queue that allows asynchronous consuming.
    /// </summary>
    /// <typeparam name="T">The type of the items contained in the queue.</typeparam>
    public class AsyncQueue<T> : AsyncCollection<T>
    {
        /// <summary>
        /// Initializes a new empty instance of <see cref="AsyncQueue{T}"/>.
        /// </summary>
        public AsyncQueue() : base(new ConcurrentQueue<T>()) { }

        /// <summary>
        /// Initializes a new instance of <see cref="AsyncQueue{T}"/> that contains elements copied from a specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new queue.</param>
        public AsyncQueue(IEnumerable<T> collection) : base(new ConcurrentQueue<T>(collection)) { }
    }
}
