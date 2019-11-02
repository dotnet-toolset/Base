using System;

namespace Base.Logging
{
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Trace = 0x01,
        Debug = 0x02,
        Info = 0x04,
        Warn = 0x08,
        Error = 0x10,
        Fatal = 0x20,

        DefaultMask = Debug | Info | Warn | Error | Fatal,
        AllMask = Trace | Debug | Info | Warn | Error | Fatal,
    }
}