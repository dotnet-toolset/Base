using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Base.Logging.Impl;

namespace Base.Logging
{
    public class LogMessage 
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

        protected LogMessage(LogLevel level, string text, object exception, DateTime stamp,  string threadName)
        {
            Level = level;
            Text=new StringBuilder(text);
            Exception = exception;
            Stamp = stamp;
            ThreadName = threadName;
        }

        public void AddPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return;
            Text.Insert(0, ' ');
            Text.Insert(0, prefix);
        }

    }
}