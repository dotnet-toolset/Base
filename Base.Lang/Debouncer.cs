using System;
using System.Threading;

namespace Base.Lang
{
    /// <summary>
    /// Pools repeated events of the same nature and fires the attached action only once after the last event.
    /// </summary>
    public class Debouncer : Disposable
    {
        private readonly Timer _timer;
        private volatile Action _action;
        private TimeSpan _delay = Timeout.InfiniteTimeSpan, _period = Timeout.InfiniteTimeSpan;
        /// <summary>
        /// if true, means the action is scheduled to run, so whenever we change timer, we must apply delay that is currently configured, so the action runs after delay.
        /// if false, we always set timer's delay to infinite, because we either haven't encountered Trigger() or Enqueue() recently, or the action has already run on delay
        /// </summary>
        private bool _shouldRunOnDelay;

        public TimeSpan Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                AdjustTimer();
            }
        }

        public TimeSpan Period
        {
            get => _period;
            set
            {
                _period = value;
                AdjustTimer();
            }
        }

        public event Action<Exception> ErrorHandler;

        public Debouncer(Action action)
        {
            _action = action;
            _timer = new Timer(Callback);
        }

        public Debouncer(TimeSpan delay)
        {
            _delay = delay;
            _timer = new Timer(Callback);
        }

        /// <summary>
        /// Delay is not applied immediately, but ONLY after Trigger() or Enqueue() is called!
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public Debouncer(Action action, TimeSpan delay)
        {
            _delay = delay;
            _action = action;
            // we don't start the timer right away, only when Trigger is called
            _timer = new Timer(Callback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Delay is not applied immediately, but ONLY after Trigger() or Enqueue() is called!
        /// Period is applied immediately, so the action will be called periodically, regardless of Trigger() or Enqueue() 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        /// <param name="period"></param>
        public Debouncer(Action action, TimeSpan delay, TimeSpan period)
        {
            _delay = delay;
            _period = period;
            _action = action;
            // we don't start the timer right away, only when Trigger is called
            _timer = new Timer(Callback, null, Timeout.InfiniteTimeSpan, _period);
        }

        protected override void OnDispose()
        {
            _timer.Dispose();
        }

        private void AdjustTimer()
        {
            _timer.Change(_shouldRunOnDelay ? _delay : Timeout.InfiniteTimeSpan, _period);
        }

        private void Callback(object state)
        {
            var action = _action;
            if (action == null || IsDisposed) return;
            try
            {
                _shouldRunOnDelay = false;
                action();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        protected virtual void OnError(Exception e)
        {
            try
            {
                ErrorHandler?.Invoke(e);
            }
            catch (Exception ex)
            {
                Bugs.Break(ex);
            }
        }

        public void Enqueue(Action a)
        {
            CheckDisposed();
            _action = a;
            _shouldRunOnDelay = a != null && _delay != Timeout.InfiniteTimeSpan;
            AdjustTimer();
        }

        public void Trigger()
        {
            CheckDisposed();
            _shouldRunOnDelay = _action != null && _delay != Timeout.InfiniteTimeSpan;
            AdjustTimer();
        }

        public void Cancel()
        {
            CheckDisposed();
            _delay = Timeout.InfiniteTimeSpan;
            _period = Timeout.InfiniteTimeSpan;
            _shouldRunOnDelay = false;
            AdjustTimer();
        }
    }
}
