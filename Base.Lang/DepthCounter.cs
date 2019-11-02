using System;
using System.Threading;

namespace Base.Lang
{
    public class DepthCounter
    {
        private int _value;

        public int Value => _value;
        
        public int? MaxDepth;
        private class Decr : Disposable
        {
            private readonly DepthCounter _counter;
            public Decr(DepthCounter counter)
            {
                _counter = counter;
                Interlocked.Increment(ref _counter._value);
            }

            protected override void OnDispose()
            {
                Interlocked.Decrement(ref _counter._value);
            }
        }

        public IDisposable Enter()
        {
            if (MaxDepth.HasValue && Value>MaxDepth) throw new CodeBug("recursion depth exceeded");
            return new Decr(this);
        }
    }
}
