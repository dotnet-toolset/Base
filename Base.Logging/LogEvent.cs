using Base.Lang;

namespace Base.Logging
{
    public abstract class LogEvent : GlobalEvent
    {
        public class CreateDefaultAppender : LogEvent
        {
            public ILogAppender Appender;

            internal CreateDefaultAppender()
            {
            }
        }

        public class GetDefaultLogFolder : LogEvent
        {
            public string Folder;

            internal GetDefaultLogFolder()
            {
            }
        }
    }
}