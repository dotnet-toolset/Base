using System;
using System.Collections.Generic;
using System.Text;

namespace Base.Logging
{
    public interface ILogFormatter
    {
        string Format(ILogger logger, LogMessage message);
    }
}
