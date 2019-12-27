using System;
using System.Collections.Generic;
using System.Text;

namespace Base.Logging
{
    /// <summary>
    /// Implementations of this interface queue log messages for writing to the actual log
    /// </summary>
    public interface ILogAppender
    {
        /// <summary>
        /// Must take as little time as possible to buffer log messages, 
        /// preferrably schedule actual writing in a separate task.
        /// </summary>
        /// <param name="logger">logger that was used to log the message</param>
        /// <param name="message">message to send to the log</param>
        void Enqueue(ILogger logger, LogMessage message);
    }
}
