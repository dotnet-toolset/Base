using System.Text;

namespace Base.Logging.Impl
{
    public class DefaultLogger : ILogger
    {
        public string Name { get; }
        public ILogger Parent { get; }
        public ILogAppender Appender { get; }

        public LogLevel LevelMask { get; set; }
        public ILogFormatter Formatter { get; set; }
        public string FullName { get; protected set; }

        public DefaultLogger(ILogger parent, string name)
        {
            Parent = parent;
            Name = name;
            Appender = parent.Appender;
            LevelMask = parent.LevelMask;
            Formatter = parent.Formatter;
            var sb = new StringBuilder();
            if (parent != null)
                sb.Append(parent.FullName).Append("/");
            sb.Append(name);
            FullName = sb.ToString();
        }

        public DefaultLogger(string name, ILogAppender appender)
        {
            Name = name;
            Appender = appender;
            LevelMask = LogLevel.DefaultMask;
            Formatter = DefaultFormatter.Instance;
            FullName = name;
        }


        public void Log(LogMessage aMessage)
        {
            if ((LevelMask & aMessage.Level) != 0)
                Appender.Enqueue(this, aMessage);
        }

        public class Instance : DefaultLogger, IInstanceLogger
        {
            public int InstanceId { get; }

            public Instance(string name, ILogAppender appender, int instanceId)
                : base(name, appender)
            {
                InstanceId = instanceId;
                FullName = FullName + "#" + InstanceId;
            }

            public Instance(ILogger parent, string name, int instanceId)
                : base(parent, name)
            {
                InstanceId = instanceId;
            }
        }
    }
}