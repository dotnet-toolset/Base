using System.Collections.Generic;
using Base.Lang;

namespace Base.Logging
{
    //
    public abstract class LogEvent : GlobalEvent
    {
        public class CreateDefaultAppender : LogEvent
        {
            public readonly List<ILogAppender> Appenders;

            internal CreateDefaultAppender(List<ILogAppender> _appenders)
            {
                Appenders = _appenders;
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