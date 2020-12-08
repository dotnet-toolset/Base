using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Base.Logging.Impl
{
    public class DefaultLogger : ILogger
    {
        private readonly IReadOnlyList<ILogAppender> _appenders;
        
        public string Name { get; }
        public ILogger Parent { get; }
        public int? InstanceId { get; }

        public IReadOnlyList<ILogAppender> Appenders =>
            _appenders ?? Parent?.Appenders ?? LogManager.GetDefaultAppenders();
        

        public LogLevel LevelMask { get; set; }
        public ILogFormatter Formatter { get; set; }
        public string FullName { get; protected set; }

        public DefaultLogger(ILogger parent, string name, int? instanceId)
        {
            Parent = parent;
            Name = name;
            LevelMask = parent.LevelMask;
            Formatter = parent.Formatter;
            InstanceId = instanceId;
            var sb = new StringBuilder();
            if (parent != null)
                sb.Append(parent.FullName).Append("/");
            sb.Append(name);
            if (instanceId.HasValue)
                sb.Append('#').Append(instanceId.Value);
            FullName = sb.ToString();
        }

        public DefaultLogger(string name, IReadOnlyList<ILogAppender> appenders, int? instanceId)
        {
            Name = name;
            _appenders = appenders;
            InstanceId = instanceId;
            LevelMask = LogLevel.DefaultMask;
            Formatter = DefaultFormatter.Instance;
            if (instanceId.HasValue)
                FullName = name + "#" + instanceId;
            else
                FullName = name;
        }


        public void Log(LogMessage aMessage)
        {
            if ((LevelMask & aMessage.Level) != 0)
                foreach (var appender in Appenders)
                    appender.Append(this, aMessage);
        }

    }
}