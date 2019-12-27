using System;
using System.Diagnostics;
using System.Threading;

namespace Base.Lang
{
    /// <summary>
    /// Convenience base class for disposable classes, ensures that <see cref="OnDispose"/> is called only once per instance,
    /// allows to auto-dispose dispose in finalizer (by setting <see cref="DisposeInFinalizer"/> property in the descendants),
    /// provides convenience methods to determine whether the instance is disposed (<see cref="IsDisposed"/>, <see cref="CheckDisposed"/>),
    /// provides <see cref="DisposeToken"/> that can be hooked to run custom code before the instance is disposed.
    ///
    /// Descendants should override <see cref="OnDispose"/> to clean up and free allocated resources.
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        private readonly CancellationTokenSource _disposeCancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Set this to <see langword="True"/>  to automatically call <see cref="Dispose"/> when the instance is finalized 
        /// </summary>
        protected bool DisposeInFinalizer;

        /// <summary>
        /// <see langword="True"/> if the instance has been disposed or is being disposed,
        /// <see langword="False"/> otherwise 
        /// </summary>
        public bool IsDisposed => _disposeCancellationTokenSource.IsCancellationRequested;

        /// <summary>
        /// Cancellation token that is cancelled right before the instance is disposed.
        /// </summary>
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

        /// <summary>
        /// Convenience method to call in the descendants to make sure the instance wasn't disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">is thrown if the instance has been disposed or is being disposed</exception>
        protected void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Override this to do custom cleanup.
        /// By default does nothing.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose()"/>
        /// </summary>
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