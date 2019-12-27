using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Base.Lang;

namespace Base.Logging
{
    /// <summary>
    /// Allows to log entry/exit to/from block of code
    /// </summary>
    /// <example>
    /// <code>
    ///  using (new LogBlock(logger, "code block")) {
    ///    .. do stuff here
    ///  }
    /// </code>
    /// will log entry and exit points using the specified logger
    /// </example>
    public class LogBlock : Disposable, ILoggable, INamed
    {
        private static int _counter;
        private readonly int _id;

        /// <summary>
        /// Instance of the logger for this <see cref="LogBlock"/>
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Name of this <see cref="LogBlock"/>
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Arguments of this <see cref="LogBlock"/>
        /// </summary>
        public string Args { get; }

        /// <summary>
        /// Stopwatch to time execution
        /// </summary>
        public Stopwatch Stopwatch { get; }

        /// <summary>
        /// Log execution time
        /// </summary>
        public bool LogTime { get; set; }

        /// <summary>
        /// Set this before the instance of this <see cref="LogBlock"/> is disposed
        /// to send result of the block execution to the logger
        /// </summary>
        public object Result = DBNull.Value;

        /// <summary>
        /// Create new instance of the <see cref="LogBlock"/>
        /// </summary>
        /// <param name="logger">logger to use for messages</param>
        /// <param name="name">name of this code block</param>
        public LogBlock(ILogger logger, string name, string args=null)
        {
            _id = Interlocked.Increment(ref _counter);
            Logger = logger;
            Name = name;
            Args = args;
            logger.Debug($"#{_id} entering {name}({args})");
            Stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Create new instance of the <see cref="LogBlock"/>
        /// </summary>
        /// <param name="loggable">loggable instance to use for messages</param>
        /// <param name="name">name of this code block</param>
        public LogBlock(ILoggable loggable, string name, string args=null)
            : this(loggable.Logger, name, args)
        {
        }

        /// <summary>
        /// Dispose this instance
        /// </summary>
        protected override void OnDispose()
        {
            Stopwatch.Stop();
            var sb = new StringBuilder("#").Append(_id).Append(" leaving ").Append(Name);
            if (LogTime)
                sb.Append(" [").Append(Stopwatch.Elapsed).Append(']');
            if (Result != DBNull.Value)
                sb.Append(", result: ").Append(Result);
            Logger.Debug(sb.ToString());
        }
    }
}