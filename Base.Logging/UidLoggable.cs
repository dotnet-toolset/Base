using System.Text;
using System.Threading;
using Base.Lang;

namespace Base.Logging
{
    /// <summary>
    /// We need to keep it generic so that the _counter static is per-generic type and not global
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UidLoggable<T> : Disposable, INamed, ILoggable
        where T : UidLoggable<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static int _counter;

        public int Uid { get; }
        public string Name { get; }
        public ILogger Logger { get; }

        protected UidLoggable(string name)
        {
            Uid = Interlocked.Increment(ref _counter);
            Name = name;
            Logger = LogManager.GetLogger(Name, Uid);
        }

        public override string ToString()
        {
            return new StringBuilder(Name).Append('#').Append(Uid).ToString();
        }
    }
}