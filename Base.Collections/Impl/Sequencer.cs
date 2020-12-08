using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Base.Lang;

namespace Base.Collections.Impl
{
    /// <summary>
    /// This is request-response async queue, matches responses to requests, allows to queue request and wait for corresponding response 
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class Sequencer<TK, TV> : Disposable
    {
        public readonly TimeSpan Timeout;
        private readonly ConcurrentDictionary<TK, Entry> _queue;

        /// <summary>
        /// Before using this, make sure that async code can't actually send response before we have started the wait. In many cases it's perfectly feasible
        /// </summary>
        public bool OmitUnexpectedNotifications;

        private class Entry : Disposable
        {
            private readonly Sequencer<TK, TV> _sequencer;
            private readonly TK _key;
            public readonly TaskCompletionSource<TV> Tcs;
            private readonly Timer _timer;

            public Entry(Sequencer<TK, TV> sequencer, TK key, bool addedByNotifier)
            {
                _sequencer = sequencer;
                _key = key;
                Tcs = new TaskCompletionSource<TV>();
                if (addedByNotifier && _sequencer.Timeout != System.Threading.Timeout.InfiniteTimeSpan)
                    _timer = new Timer(s => Dispose(), null, _sequencer.Timeout.Multiply(2),
                        System.Threading.Timeout.InfiniteTimeSpan);
            }

            protected override void OnDispose()
            {
                _timer?.Dispose();
                _sequencer._queue.Remove(_key);
            }

            public async Task<TV> When(CancellationToken ct)
            {
                var timeout = Task.Delay(_sequencer.Timeout, ct);
                try
                {
                    var result = await Task.WhenAny(timeout, Tcs.Task);
                    if (result == timeout) throw new TimeoutException();
                    return await Tcs.Task;
                }
                finally
                {
                    Dispose();
                }
            }
        }

        public Sequencer(TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero)
                throw new ArgumentException("sequencer timeout must be either infinite or positive", nameof(timeout));
            Timeout = timeout;
            _queue = new ConcurrentDictionary<TK, Entry>();
        }

        protected override void OnDispose()
        {
            _queue.Clear();
        }

        public async Task<TV> When(TK key, CancellationToken ct)
        {
            using (var rct = CancellationTokenSource.CreateLinkedTokenSource(DisposeToken, ct))
            {
                rct.Token.ThrowIfCancellationRequested();
                return await _queue.GetOrAdd(key, k => new Entry(this, k, false)).When(rct.Token);
            }
        }

        public void Notify(TK key, TV value)
        {
            CheckDisposed();
            Entry entry;
            if (OmitUnexpectedNotifications)
                _queue.TryGetValue(key, out entry);
            else
                entry = _queue.GetOrAdd(key, k => new Entry(this, k, true));
            entry?.Tcs?.TrySetResult(value);
            // don't dispose entry here, someone may start waiting AFTER we notify
        }

        public void Error(Exception e)
        {
            foreach (var entry in _queue.Values.ToArray())
            {
                entry.Tcs?.TrySetException(e);
                entry.Dispose();
            }
        }
    }
}