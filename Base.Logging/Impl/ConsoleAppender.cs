using System;
using System.Collections.Generic;
using System.Text;

namespace Base.Logging.Impl
{
    public class ConsoleAppender : ILogAppender
    {
        public void Append(ILogger logger, LogMessage message)
        {
            Console.WriteLine(logger.Formatter.Format(logger, message));
        }
            
    }
}
