using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base.Collections.Async
{
    public interface IAwaiter<T>
    {
        /// <summary>
        /// <para>Attempts to complete the awaiter with a specified result.</para>
        /// <para>Returns false if the awaiter has been canceled.</para>
        /// </summary>
        bool TrySetResult(T result);

        /// <summary>
        /// The task that's completed when the awaiter gets the result.
        /// </summary>
        Task<T> Task { get; }
    }

    public interface IAwaiterFactory<T>
    {
        IAwaiter<T> CreateAwaiter();
    }

    public struct InstanceAwaiterFactory<T> : IAwaiterFactory<T>, IEquatable<InstanceAwaiterFactory<T>>
    {
        private readonly IAwaiter<T> _awaiter;

        public InstanceAwaiterFactory(IAwaiter<T> awaiter)
        {
            _awaiter = awaiter;
        }

        public IAwaiter<T> CreateAwaiter() => _awaiter;

        #region IEquatable<InstanceAwaiterFactory<T>>

        public override int GetHashCode() => EqualityComparer<IAwaiter<T>>.Default.GetHashCode(_awaiter);
        public bool Equals(InstanceAwaiterFactory<T> other) => EqualityComparer<IAwaiter<T>>.Default.Equals(_awaiter, other._awaiter);
        public override bool Equals(object obj) => obj is InstanceAwaiterFactory<T> && Equals((InstanceAwaiterFactory<T>)obj);

        public static bool operator ==(InstanceAwaiterFactory<T> x, InstanceAwaiterFactory<T> y) => x.Equals(y);
        public static bool operator !=(InstanceAwaiterFactory<T> x, InstanceAwaiterFactory<T> y) => !x.Equals(y);

        #endregion
    }
}
