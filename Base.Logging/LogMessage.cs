using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Base.Lang.Transform;
using Base.Logging.Impl;

namespace Base.Logging
{
    public class LogMessage : ISimplifiable
    {
        public readonly LogLevel Level;
        public readonly StringBuilder Text;
        public readonly object Exception;
        public readonly DateTime Stamp;
        public readonly string ThreadName;

        public LogMessage(LogLevel level, object message, Exception exception = null)
        {
            Level = level;
            Exception = exception;

            var ex = message as Exception;
            Text = new StringBuilder(ex?.GetBaseException().Message ?? Convert.ToString(message));
            Exception = exception ?? ex;
            Stamp = DateTime.UtcNow;
            var vThread = Thread.CurrentThread;
            var mid = Convert.ToString(vThread.ManagedThreadId);
            ThreadName = vThread.Name ?? mid;
            if (ThreadName == "Threadpool worker" || vThread.IsThreadPoolThread) ThreadName = "p" + mid;
        }

        public LogMessage(IReadOnlyDictionary<string, object> simple)
        {
            Text = new StringBuilder();
            foreach (var kv in simple)
                switch (kv.Key)
                {
                    case "level":
                        Level = LogUtils.NameToLevel(Convert.ToString(kv.Value));
                        break;
                    case "text":
                        Text.Append(kv.Value);
                        break;
                    case "stamp":
                        DateTime.TryParseExact(Convert.ToString(kv.Value), "s", DateTimeFormatInfo.InvariantInfo,
                            DateTimeStyles.None, out Stamp);
                        break;
                    case "thread":
                        ThreadName = Convert.ToString(kv.Value);
                        break;
                    case "exception":
                        Exception = kv.Value;
                        break;
                }
        }

        public void AddPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return;
            Text.Insert(0, ' ');
            Text.Insert(0, prefix);
        }

        public object Simplify()
        {
            var result = new Dictionary<string, object>
            {
                ["level"] = Level.GetName(),
                ["text"] = Text.ToString(),
                ["stamp"] = Stamp.ToString("s"),
                ["thread"] = ThreadName
            };
            if (Exception != null)
                result["exception"] = Exception.ToString();
            return result;
        }
    }
}