using System;
using System.Collections.Generic;
using System.Text;

namespace Base.Logging
{
    /// <summary>
    /// This interface may be used to perform custom log message formatting
    /// </summary>
    public interface ILogFormatter
    {
        /// <summary>
        /// Format log message for writing into the log
        /// </summary>
        /// <param name="logger">logger instace</param>
        /// <param name="message">message to format</param>
        /// <returns>formatted message</returns>
        string Format(ILogger logger, LogMessage message);
    }
}
