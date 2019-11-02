using System;
using System.IO;
using Base.Collections;
using Base.Collections.Impl;
using Base.Logging.Impl;

namespace Base.Logging
{
    public static class Extensions
    {
        private static readonly ICache<ILogger, ICache<string, ILogger>> ChildLoggers =
            new WeakCache<ILogger, ICache<string, ILogger>>();

        public static ILogger GetLogger(this ILogger parent, string name)
        {
            return ChildLoggers.Get(parent, l => new WeakCache<string, ILogger>())
                .Get(name, n => new DefaultLogger(parent, name));
        }

        public static ILogger GetLogger(this ILogger parent, string name, int uid)
        {
            var key = name + "#" + uid;
            return ChildLoggers.Get(parent, l => new WeakCache<string, ILogger>())
                .Get(key, n => new DefaultLogger.Instance(parent, name, uid));
        }

        public static string GetName(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return "trace";
                case LogLevel.Debug:
                    return "debug";
                case LogLevel.Info:
                    return "info";
                case LogLevel.Warn:
                    return "warn";
                case LogLevel.Error:
                    return "error";
                case LogLevel.Fatal:
                    return "fatal";
                default:
                    return Convert.ToString((int) level);
            }
        }

        public static bool IsEnabled(this ILogger logger, LogLevel iLevel) => (logger?.LevelMask & iLevel) != 0;

        public static bool IsTraceEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Trace);

        public static bool IsDebugEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Debug);

        public static bool IsInfoEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Info);

        public static bool IsWarnEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Warn);

        public static bool IsErrorEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Error);

        public static bool IsFatalEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Fatal);

        public static void Trace(this ILogger logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Trace, message));
        }

        public static void Trace(this ILogger logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Trace, message, exception));
        }

        public static void TraceFormat(this ILogger logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Trace, string.Format(format, args)));
        }

        public static void Debug(this ILogger logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Debug, message));
        }

        public static void Debug(this ILogger logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Debug, message, exception));
        }

        public static void DebugFormat(this ILogger logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Debug, string.Format(format, args)));
        }

        public static void Info(this ILogger logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Info, message));
        }

        public static void Info(this ILogger logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Info, message, exception));
        }

        public static void InfoFormat(this ILogger logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Info, string.Format(format, args)));
        }

        public static void Warn(this ILogger logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Warn, message));
        }

        public static void Warn(this ILogger logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Warn, message, exception));
        }

        public static void WarnFormat(this ILogger logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Warn, string.Format(format, args)));
        }

        public static bool AssertWarn(this ILogger logger, bool condition, object message)
        {
            if (!condition && message != null)
                Log(logger, new LogMessage(LogLevel.Warn, message));
            return condition;
        }

        public static bool AssertWarn(this ILogger logger, bool condition, object message, Exception exception)
        {
            if (!condition && message != null)
                Log(logger, new LogMessage(LogLevel.Warn, message, exception));
            return condition;
        }

        public static bool AssertWarn(this ILogger logger, bool condition, string format, params object[] args)
        {
            if (!condition && format != null)
                Log(logger, new LogMessage(LogLevel.Warn, string.Format(format, args)));
            return condition;
        }


        public static void Error(this ILogger logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Error, message));
        }

        public static void Error(this ILogger logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Error, message, exception));
        }

        public static void ErrorFormat(this ILogger logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Error, string.Format(format, args)));
        }

        public static void Fatal(this ILogger logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Fatal, message));
        }

        public static void Fatal(this ILogger logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Fatal, message, exception));
        }

        public static void FatalFormat(this ILogger logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Fatal, string.Format(format, args)));
        }

        public static void Log(this ILogger logger, LogLevel level, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(level, string.Format(format, args)));
        }

        public static void Log(this ILogger logger, LogMessage aMessage)
        {
            if (logger != null)
                try
                {
                    logger.Log(aMessage);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
        }

        public static void DumpBytes(this ILogger logger, byte[] buf, string message, LogLevel level = LogLevel.Info,
            int ofs = 0, int length = -1)
        {
            if (logger == null) return;
            var msg = new LogMessage(level, message);
            if (buf != null)
            {
                var len = length < 0 ? buf.Length : length;
                LogUtils.DumpBytes(new StringWriter(msg.Text), buf, ofs, len);
            }

            Log(logger, msg);
        }

        public static void DumpBytes(this ILogger logger, ArraySegment<byte> buf, string message,
            LogLevel level = LogLevel.Info)
        {
            logger.DumpBytes(buf.Array, message, level, buf.Offset, buf.Count);
        }

        public static bool IsEnabled(this ILoggable logger, LogLevel iLevel) =>
            (logger?.Logger?.LevelMask & iLevel) != 0;

        public static bool IsTraceEnabled(this ILoggable logger) => (logger?.Logger?.LevelMask & LogLevel.Trace) != 0;

        public static bool IsDebugEnabled(this ILoggable logger) => (logger?.Logger?.LevelMask & LogLevel.Debug) != 0;
        public static bool IsInfoEnabled(this ILoggable logger) => (logger?.Logger?.LevelMask & LogLevel.Info) != 0;

        public static bool IsWarnEnabled(this ILoggable logger) => (logger?.Logger?.LevelMask & LogLevel.Warn) != 0;

        public static bool IsErrorEnabled(this ILoggable logger) => (logger?.Logger?.LevelMask & LogLevel.Error) != 0;

        public static bool IsFatalEnabled(this ILoggable logger) => (logger?.Logger?.LevelMask & LogLevel.Fatal) != 0;

        public static void Trace(this ILoggable logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Trace, message));
        }

        public static void Trace(this ILoggable logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Trace, message, exception));
        }

        public static void TraceFormat(this ILoggable logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Trace, string.Format(format, args)));
        }

        public static void Debug(this ILoggable logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Debug, message));
        }

        public static void Debug(this ILoggable logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Debug, message, exception));
        }

        public static void DebugFormat(this ILoggable logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Debug, string.Format(format, args)));
        }

        public static void Info(this ILoggable logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Info, message));
        }

        public static void Info(this ILoggable logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Info, message, exception));
        }

        public static void InfoFormat(this ILoggable logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Info, string.Format(format, args)));
        }

        public static void Warn(this ILoggable logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Warn, message));
        }

        public static void Warn(this ILoggable logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Warn, message, exception));
        }

        public static void WarnFormat(this ILoggable logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Warn, string.Format(format, args)));
        }

        public static void Error(this ILoggable logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Error, message));
        }

        public static void Error(this ILoggable logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Error, message, exception));
        }

        public static void ErrorFormat(this ILoggable logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Error, string.Format(format, args)));
        }

        public static void Fatal(this ILoggable logger, object message)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Fatal, message));
        }

        public static void Fatal(this ILoggable logger, object message, Exception exception)
        {
            if (message != null)
                Log(logger, new LogMessage(LogLevel.Fatal, message, exception));
        }

        public static void FatalFormat(this ILoggable logger, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(LogLevel.Fatal, string.Format(format, args)));
        }

        public static void Log(this ILoggable logger, LogLevel level, string format, params object[] args)
        {
            if (format != null)
                Log(logger, new LogMessage(level, string.Format(format, args)));
        }

        public static void Log(this ILoggable logger, LogMessage aMessage)
        {
            if (logger != null)
                try
                {
                    logger.Logger.Log(aMessage);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
        }

        public static void DumpBytes(this ILoggable loggable, byte[] buf, string message,
            LogLevel level = LogLevel.Info, int ofs = 0, int length = -1)
        {
            loggable?.Logger.DumpBytes(buf, message, level, ofs, length);
        }

        public static void DumpBytes(this ILoggable loggable, ArraySegment<byte> buf, string message,
            LogLevel level = LogLevel.Info)
        {
            loggable?.Logger.DumpBytes(buf, message, level);
        }
    }
}