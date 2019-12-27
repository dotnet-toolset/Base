using Base.Collections.Impl.Async;
using Base.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Logging.Impl
{
    public abstract class AbstractAppender : Disposable, ILogAppender
    {
        private readonly AsyncQueue<Tuple<ILogger, LogMessage>> _queue = new AsyncQueue<Tuple<ILogger, LogMessage>>();
        private readonly Task _task;
        private readonly Debouncer _flushDebouncer;
        private bool _flushImmediately;

        protected AbstractAppender()
        {
            _task = Task.Run(async () =>
            {
                while (!IsDisposed)
                    try
                    {
                        var taken = await _queue.TakeAsync(DisposeToken);
                        if (taken.End) break;
                        var tuple = taken.Value;
                        if (tuple.Item1 == null && tuple.Item2 == null)
                            await FlushAsync(DisposeToken);
                        else
                        {
                            await AppendAsync(tuple.Item1, tuple.Item2, DisposeToken);
                            if (_flushImmediately)
                                await FlushAsync(CancellationToken.None); // we flush even cancellation (which is disposal in fact) is in progress
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception e)
                    {
                        Bugs.LastResortEmergencyLog(e);
                    }
            });
            _flushDebouncer = new Debouncer(() => _queue.Add(Tuple.Create<ILogger, LogMessage>(null, null)), TimeSpan.FromMilliseconds(200));

            GlobalEvent.Emitter.OfType<GlobalEvent.ProcessExit>().Take(1).Subscribe(e =>
            {
                Dispose();
            });

        }

        protected override void OnDispose()
        {
            _flushImmediately = true;
            try
            {
                _task.Wait(TimeSpan.FromSeconds(10));
                var remaining = _queue.ToList();
                _queue.Dispose();
                if (remaining.Count > 0) Task.Run(async () =>
                {
                    try
                    {
                        foreach (var tuple in remaining)
                            await AppendAsync(tuple.Item1, tuple.Item2, CancellationToken.None);
                        await FlushAsync(CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }).Wait(TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Enqueue(ILogger logger, LogMessage message)
        {
            if (IsDisposed) return;
            _queue.Add(Tuple.Create(logger, message));
            _flushDebouncer.Trigger();
        }

        protected abstract Task AppendAsync(ILogger logger, LogMessage message, CancellationToken ct);
        protected virtual Task FlushAsync(CancellationToken ct) { return Task.CompletedTask; }
    }
}
