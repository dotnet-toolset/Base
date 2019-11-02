using System;
using System.Diagnostics;
using System.Threading;

namespace Base.Lang
{
    public abstract class Disposable : IDisposable
    {
        private readonly CancellationTokenSource _disposeCancellationTokenSource = new CancellationTokenSource();
        protected bool DisposeInFinalizer;

        public bool IsDisposed => _disposeCancellationTokenSource.IsCancellationRequested;
        public CancellationToken DisposeToken => _disposeCancellationTokenSource.Token;

        ~Disposable()
        {
            if (DisposeInFinalizer)
                try
                {
                    Dispose();
                }
                catch (Exception e)
                {
                    try
                    {
                        Trace.WriteLine("exception in finalizer: " + e);
                    }
                    catch
                    {
                    }
                }
        }

        protected void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void OnDispose()
        {
        }

        public void Dispose()
        {
            // locking is necessary because someone may intervene between _disposeCancellationTokenSource.Cancel() and _disposeCancellationTokenSource.IsCancellationRequested
            try
            {
                lock (this)
                {
                    if (IsDisposed) return;
                    if (DisposeInFinalizer) GC.SuppressFinalize(this);
                    _disposeCancellationTokenSource.Cancel();
                }

                OnDispose();
            }
            catch (Exception e)
            {
                Trace.WriteLine("exception in Dispose(): " + e);
            }
        }
    }
}