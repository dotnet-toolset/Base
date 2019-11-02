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
        public string FullName { get; }

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
            sb.Append(MakeFullName());
            FullName = sb.ToString();
        }

        public DefaultLogger(string name, ILogAppender appender)
        {
            Name = name;
            Appender = appender;
            LevelMask = LogLevel.DefaultMask;
            Formatter = DefaultFormatter.Instance;
            FullName = MakeFullName();
        }

        protected virtual string MakeFullName()
        {
            return Name;
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
            }

            public Instance(ILogger parent, string name, int instanceId)
                : base(parent, name)
            {
                InstanceId = instanceId;
            }

            protected override string MakeFullName()
            {
                return Name + "#" + InstanceId;
            }
        }
    }
}