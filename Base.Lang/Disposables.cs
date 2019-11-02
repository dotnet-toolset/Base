using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Base.Lang
{
    public class Disposables : Disposable
    {
        private readonly ConcurrentBag<IDisposable> _bag = new ConcurrentBag<IDisposable>();
        public Disposables() { }
        public Disposables(IEnumerable<IDisposable> list)
        {
            foreach (var d in list)
                _bag.Add(d);
        }
        public void Add(IDisposable disposable)
        {
            _bag.Add(disposable);
        }

        protected override void OnDispose()
        {
            while (_bag.TryTake(out var obj))
                obj?.Dispose();
            base.OnDispose();
        }
    }
}
