using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Base.Collections;
using Base.Collections.Impl;
using Base.Lang;
using Base.Logging.Impl;

namespace Base.Logging
{
    public static class LogManager
    {
        private static readonly ICache<string, ILogger> LoggersByName = new WeakCache<string, ILogger>();
        private static readonly ICache<string, FileAppender> _fileAppenders = new WeakCache<string, FileAppender>();

        private static IReadOnlyList<ILogAppender> _defaultAppenders;
        private static string _defaultLogFolder;
        public static readonly ILogger Default;

        static LogManager()
        {
            Default = GetLogger("system");
#if NETFRAMEWORK
            Debug.Listeners.Add(new DebugLog(true));
#endif
            Trace.Listeners.Add(new DebugLog(false));
        }

        public static ILogAppender GetFileAppender(string folder, string name, bool append)
        {
            if (name == null) name = "log";
            var p = Path.GetFullPath(Path.Combine(folder, name));
            return _fileAppenders.Get(p, k => new FileAppender(folder, name, append));
        }

        public static string GetDefaultLogFolder()
        {
            if (_defaultLogFolder == null)
                lock (typeof(LogManager))
                    if (_defaultLogFolder == null)
                    {
                        var folder = AppUtils.GetApplicationProduct() ?? "app";
                        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        if (appdata != null) folder = Path.Combine(appdata, folder);
                        var ev = GlobalEvent.Fire(new LogEvent.GetDefaultLogFolder {Folder = folder});
                        _defaultLogFolder = ev.Folder ?? folder;
                    }

            return _defaultLogFolder;
        }

        public static IReadOnlyList<ILogAppender> GetDefaultAppenders()
        {
            if (_defaultAppenders == null)
                lock (typeof(LogManager))
                    if (_defaultAppenders == null)
                    {
                        var ev = GlobalEvent.Fire(new LogEvent.CreateDefaultAppender(new List<ILogAppender>
                        {
                            GetFileAppender(GetDefaultLogFolder(), AppUtils.GetApplicationName(),
                                false)
                        }));
                        _defaultAppenders = ev.Appenders;
                    }

            return _defaultAppenders;
        }

        public static ILogger GetLogger(string name) =>
            LoggersByName.Get(name, n => new DefaultLogger(n, GetDefaultAppenders(), null));

        public static ILogger GetLogger(string name, int uid) => LoggersByName.Get(name + "#" + uid,
            n => new DefaultLogger(name, GetDefaultAppenders(), uid));


        private static readonly ThreadLocal<int> LoggableRecursionGuard = new ThreadLocal<int>(() => 0);

        public static ILogger GetLogger(ILoggable instance)
        {
            ILogger result;
            if (instance == null) return null;
            var depth = LoggableRecursionGuard.Value;
            try
            {
                LoggableRecursionGuard.Value++;
                if (depth == 0)
                {
                    result = instance.Logger;
                    if (result != null) return result;
                }

                var name = (instance as INamed)?.Name ?? Convert.ToString(instance);
                result = new DefaultLogger(name, GetDefaultAppenders(), null);
            }
            finally
            {
                LoggableRecursionGuard.Value = depth;
            }

            return result;
        }

        public static ILogger GetFileLogger(string abase, string name, bool aAppend = false)
        {
            var key = "file://" + Path.GetFullPath(Path.Combine(abase, name));
            return LoggersByName.Get(key,
                k => new DefaultLogger(name, new[] {GetFileAppender(abase, name, aAppend)}, null));
        }

        public static ILogger GetFileLogger(string name, bool aAppend = false)
        {
            return GetFileLogger(GetDefaultLogFolder(), name, aAppend);
        }

        class DebugLog : TraceListener
        {
            private readonly ILogger _logger;
            private readonly bool _debug;

            internal DebugLog(bool debug)
            {
                if (_debug = debug)
                {
                    var name = AppUtils.GetApplicationName();
                    if (name == null) name = "debug";
                    else name += "-debug";
                    _logger = GetFileLogger(GetDefaultLogFolder(), name);
                }
                else _logger = GetLogger("tracer");

                _logger.LevelMask = LogLevel.AllMask;
            }

            public override void Write(string message)
            {
                try
                {
                    _logger.Log(_debug ? LogLevel.Debug : LogLevel.Trace, message);
                }
                catch (Exception e)
                {
                    Bugs.LastResortEmergencyLog(e);
                }
            }

            public override void WriteLine(string message)
            {
                try
                {
                    _logger.Log(_debug ? LogLevel.Debug : LogLevel.Trace, message);
                }
                catch (Exception e)
                {
                    Bugs.LastResortEmergencyLog(e);
                }
            }
        }
    }
}