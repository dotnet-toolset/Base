using System;
using System.Collections.Generic;
using System.Text;

namespace Base.Logging
{
    public interface ILogAppender
    {
        void Enqueue(ILogger logger, LogMessage message);
    }
}
