using System;
using System.Collections.Generic;
using System.Text;

namespace Base.Logging.Impl
{
    public class DefaultFormatter : ILogFormatter
    {
        public static readonly ILogFormatter Instance = new DefaultFormatter();

        public string Format(ILogger logger, LogMessage message)
        {
            var sb = new StringBuilder();
            sb.Append(message.Stamp.ToString("s")).Append(' ');
            if (!string.IsNullOrEmpty(message.ThreadName))
                sb.Append(message.ThreadName).Append(' ');
            sb.Append(message.Level.GetName()).Append(' ').Append(logger.FullName);
            sb.Append("  ").Append(message.Text);
            return sb.ToString();
        }
    }
}
