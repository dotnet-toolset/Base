using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base.Collections.Async;

namespace Base.Collections.Impl.Async
{
    /// <summary>
    /// Represents a thread-safe collection that allows asynchronous consuming.
    /// </summary>
    /// <typeparam name="T">The type of the items contained in the collection.</typeparam>
    public class AsyncCollection<T> : IDisposable, IAsyncCollection<T>
    {
        private readonly IProducerConsumerCollection<T> _itemQueue;
        private readonly ConcurrentQueue<IAwaiter<TakeResult<T>>> _awaiterQueue = new ConcurrentQueue<IAwaiter<TakeResult<T>>>();

        //	_queueBalance < 0 means there are free awaiters and not enough items.
        //	_queueBalance > 0 means the opposite is true.
        private long _queueBalance = 0;

        private bool _addingComplete;

        /// <summary>
        /// Initializes a new instance of <see cref="AsyncCollection{T}"/> with a specified <see cref="IProducerConsumerCollection{T}"/> as an underlying item storage.
        /// </summary>
        /// <param name="itemQueue">The collection to use as an underlying item storage. MUST NOT be accessed elsewhere.</param>
        protected AsyncCollection(IProducerConsumerCollection<T> itemQueue)
        {
            _itemQueue = itemQueue;
            _queueBalance = _itemQueue.Count;
        }

        public void Dispose()
        {
            CompleteAdding();
        }

        #region IAsyncCollection<T> members

        /// <summary>
        /// Gets an amount of pending item requests.
        /// </summary>
        public int AwaiterCount => _awaiterQueue.Count;

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        public void Add(T item)
        {
            if (_addingComplete) return;
            while (!TryAdd(item)) ;
        }

        public void CompleteAdding()
        {
            _addingComplete = true;
            while (_awaiterQueue.TryDequeue(out var awaiter))
                awaiter.TrySetResult(TakeResult<T>.CompleteInstance);
        }

        /// <summary>
        /// Tries to add an item to the collection.
        /// May fail if an awaiter that's supposed to receive the item is cancelled. If this is the case, the TryAdd() method must be called again.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        /// <returns>True if the item was added to the collection; false if the awaiter was cancelled and the operation must be retried.</returns>
        private bool TryAdd(T item)
        {
            var balanceAfterCurrentItem = Interlocked.Increment(ref _queueBalance);
            var spin = new SpinWait();

            if (balanceAfterCurrentItem > 0)
            {
                //	Items are dominating, so we can safely add a new item to the queue.
                while (!_itemQueue.TryAdd(item))
                    spin.SpinOnce();

                return true;
            }
            else
            {
                //	There's at least one awaiter available or being added as we're speaking, so we're giving the item to it.

                IAwaiter<TakeResult<T>> awaiter;

                while (!_awaiterQueue.TryDequeue(out awaiter))
                    spin.SpinOnce();

                //	Returns false if the cancellation occurred earlier.
                return awaiter.TrySetResult(new TakeResult<T>(item));
            }
        }

        /// <summary>
        /// Removes and returns an item from the collection in an asynchronous manner.
        /// </summary>
        public Task<TakeResult<T>> TakeAsync(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return Task.FromCanceled<TakeResult<T>>(ct);
            return TakeAsync(new CompletionSourceAwaiterFactory<TakeResult<T>>(ct));
        }

        public TakeResult<T> Take()
        {
            var spin = new SpinWait();
            while (!_addingComplete)
            {
                if (_itemQueue.TryTake(out var item)) return new TakeResult<T>(item);
                spin.SpinOnce();
            }
            return TakeResult<T>.CompleteInstance;
        }

        private Task<TakeResult<T>> TakeAsync<TAwaiterFactory>(TAwaiterFactory awaiterFactory) where TAwaiterFactory : IAwaiterFactory<TakeResult<T>>
        {
            var balanceAfterCurrentAwaiter = Interlocked.Decrement(ref _queueBalance);

            if (balanceAfterCurrentAwaiter < 0)
            {
                //	Awaiters are dominating, so we can safely add a new awaiter to the queue.
                var awaiter = awaiterFactory.CreateAwaiter();
                _awaiterQueue.Enqueue(awaiter);
                return awaiter.Task;
            }
            else
            {
                //	There's at least one item available or being added, so we're returning it directly.

                T item;
                var spin = new SpinWait();

                while (!_itemQueue.TryTake(out item))
                    spin.SpinOnce();

                return Task.FromResult(new TakeResult<T>(item));
            }
        }

        public override string ToString()
        {
            return $"Count = {Count}, Awaiters = {AwaiterCount}";
        }

        #endregion

        #region Static

        internal const int TakeFromAnyMaxCollections = BitArray32.BitCapacity;

        /// <summary>
        /// Removes and returns an item from one of the specified collections in an asynchronous manner.
        /// </summary>
        public static Task<AnyResult<TakeResult<T>>> TakeFromAnyAsync(AsyncCollection<T>[] collections)
        {
            return TakeFromAnyAsync(collections, CancellationToken.None);
        }

        /// <summary>
        /// Removes and returns an item from one of the specified collections in an asynchronous manner.
        /// </summary>
        public static Task<AnyResult<TakeResult<T>>> TakeFromAnyAsync(AsyncCollection<T>[] collections, CancellationToken cancellationToken)
        {
            if (collections == null)
                throw new ArgumentNullException(nameof(collections));

            if (collections.Length <= 0 || collections.Length > TakeFromAnyMaxCollections)
                throw new ArgumentException(
                    $"The collection array can't contain less than 1 or more than {TakeFromAnyMaxCollections} collections.", nameof(collections));

            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<AnyResult<TakeResult<T>>>(cancellationToken);

            var exclusiveSources = new ExclusiveCompletionSourceGroup<TakeResult<T>>();

            //	Fast route: we attempt to take from the top-priority queues that have any items.
            //	If the fast route succeeds, we avoid allocating and queueing a bunch of awaiters.
            for (var i = 0; i < collections.Length; i++)
            {
                if (collections[i].Count > 0)
                {
                    var result = TryTakeFast(exclusiveSources, collections[i], i);
                    if (result.HasValue)
                        return Task.FromResult(result.Value);
                }
            }

            //	No luck during the fast route; just queue the rest of awaiters.
            for (var i = 0; i < collections.Length; i++)
            {
                var result = TryTakeFast(exclusiveSources, collections[i], i);
                if (result.HasValue)
                    return Task.FromResult(result.Value);
            }

            //	None of the collections had any items. The order doesn't matter anymore, it's time to start the competition.
            exclusiveSources.UnlockCompetition(cancellationToken);
            return exclusiveSources.Task;
        }

        private static AnyResult<TakeResult<T>>? TryTakeFast(ExclusiveCompletionSourceGroup<TakeResult<T>> exclusiveSources, AsyncCollection<T> collection, int index)
        {
            //	This can happen if the awaiter has already been created during the fast route.
            if (exclusiveSources.IsAwaiterCreated(index))
                return null;

            var collectionTask = collection.TakeAsync(exclusiveSources.CreateAwaiterFactory(index));

            //	One of the collections already had an item and returned it directly
            if (collectionTask != null && collectionTask.IsCompleted)
            {
                exclusiveSources.MarkAsResolved();
                return new AnyResult<TakeResult<T>>(collectionTask.Result, index);
            }
            return null;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _itemQueue.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _itemQueue.GetEnumerator();
        }

        #endregion

        #region ICollection Members

        public int Count => _itemQueue.Count;

        public void CopyTo(Array array, int index)
        {
            _itemQueue.CopyTo(array, index);
        }

        bool System.Collections.ICollection.IsSynchronized => false;

        object System.Collections.ICollection.SyncRoot => throw new NotSupportedException();

        #endregion
    }

}
