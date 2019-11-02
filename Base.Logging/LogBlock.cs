using System;
using System.Linq;
using System.Text;
using System.Threading;
using Base.Lang;

namespace Base.Logging
{
    public class LogBlock : Disposable, ILoggable, INamed
    {
        private static int _counter;
        private readonly int _id;

        public ILogger Logger { get; }
        public string Name { get; }

        public object Result=DBNull.Value;

        public LogBlock(ILogger logger, string name, params object[] args)
        {
            _id = Interlocked.Increment(ref _counter);
            Logger = logger;
            Name = name;
            logger.DebugFormat("#{0} entering {1}({2})", _id, name, args.Select(o => Convert.ToString(o)).Join(","));
        }

        public LogBlock(ILoggable loggable, string name, params object[] args)
            : this(loggable.Logger, name, args)
        {
        }

        protected override void OnDispose()
        {
            var sb = new StringBuilder("#").Append(_id).Append(" leaving ").Append(Name);
            if (Result != DBNull.Value)
                sb.Append(", result: ").Append(Result);
            Logger.Debug(sb.ToString());
        }
    }
}