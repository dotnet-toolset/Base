using Base.Lang;

namespace Base.Logging
{
    /// <summary>
    /// This is the API of the logger. 
    /// Obtain instance of the logger via GetLogger(...) methods in <see cref="LogManager"/>
    /// </summary>
    public interface ILogger : INamed
    {
        /// <summary>
        /// Parent logger or <see langword="null"/> if there's no parent.
        /// Names of all parents are reflected in the log
        /// </summary>
        ILogger Parent { get; }
        
        /// <summary>
        /// Number of instance <see langword="null"/> if there's no relation to a particular instance.
        /// This is used to differentiate between instances of the same class.
        /// For example, when logging messages from multiple server connections, instance ID helps to identify a particular connection.
        /// </summary>
        int? InstanceId { get; }
        
        /// <summary>
        /// Appender to use for this logger
        /// </summary>
        ILogAppender Appender { get; }
        
        /// <summary>
        /// Log messages with these levels only
        /// </summary>
        LogLevel LevelMask { get; set; }
        
        /// <summary>
        /// Formetter to use for this logger
        /// </summary>
        ILogFormatter Formatter { get; set; }
        
        /// <summary>
        /// Full name of this logger, consists of parent names (if applicable), own name, instance ID
        /// </summary>
        string FullName { get; }
        
        /// <summary>
        /// Queues the message.
        /// </summary>
        /// <param name="message">message</param>
        void Log(LogMessage message);
    }

    /// <summary>
    /// Base interface for classes that support logging
    /// </summary>
    public interface ILoggable
    {
        /// <summary>
        /// Logger for this particular instance of the loggable object
        /// </summary>
        ILogger Logger { get; }
    }
}