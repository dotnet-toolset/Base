using System;
using System.Threading;

namespace Base.Collections.Props
{
    public interface IPropKey
    {
        object GetObject(IPropsContainer c);
        void SetObject(IPropsContainer c, object value);
    }

    public sealed class PropKey<TC, TV> : IPropKey
        where TC : IPropsContainer
    {
        private readonly int _id;

        public TV this[TC c]
        {
            get
            {
                var dp = c?.GetProps(false);
                return dp != null && dp.TryGetValue(_id, out var result) ? (TV) result : default;
            }
            set => c.GetProps(true)[_id] = value;
        }

        public PropKey() => _id = Interlocked.Increment(ref Props.Counter);

        public TV Get(TC c, TV d)
        {
            var dp = c?.GetProps(false);
            return dp != null && dp.TryGetValue(_id, out var result) ? (TV) result : d;
        }

        public TV GetOrAdd(TC c, Func<TV> creator) => (TV) c.GetProps(true).GetOrAdd(_id, k => creator());
        public bool TryAdd(TC c, TV value) => c.GetProps(true).TryAdd(_id, value);

        public TV Remove(TC c)
        {
            var props = c.GetProps(false);
            if (props != null && props.TryRemove(_id, out var result)) return (TV) result;
            return default;
        }

        public object GetObject(IPropsContainer c)
        {
            var dp = c?.GetProps(false);
            return dp != null && dp.TryGetValue(_id, out var result) ? result : null;
        }

        public void SetObject(IPropsContainer c, object value)
        {
            c.GetProps(true)[_id] = value;
        }
    }
}