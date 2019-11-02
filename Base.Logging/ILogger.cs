using Base.Lang;

namespace Base.Logging
{
    public interface ILogger : INamed
    {
        ILogger Parent { get; }
        ILogAppender Appender { get; }
        LogLevel LevelMask { get; set; }
        ILogFormatter Formatter { get; set; }
        string FullName { get; }
        void Log(LogMessage aMessage);
    }

    public interface IInstanceLogger : ILogger
    {
        int InstanceId { get; }
    }

    public interface ILoggable
    {
        ILogger Logger { get; }
    }
}