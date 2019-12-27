using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Base.Lang
{
    public static class Bugs
    {
        public static void LastResortEmergencyLog(object message)
        {
            try
            {
                Console.WriteLine(Convert.ToString(message));
            }
            catch
            {
            }
        }
        

        [ExcludeFromCodeCoverage]
        public static void Check(bool assertion, string msg = null)
        {
            if (!assertion)
                throw new CodeBug(msg ?? "assertion failed");
        }

        [Conditional("DEBUG")]
        public static void Break()
        {
            Debugger.Break();
        }

        [Conditional("DEBUG")]
        public static void Break(string message)
        {
            LastResortEmergencyLog("BUG: " + message);
            LastResortEmergencyLog(new StackTrace());
            Debug.WriteLine(message);
            Debugger.Log(0, "debug", message);
            Debugger.Break();
        }

        [Conditional("DEBUG")]
        public static void Break(Exception e)
        {
            Break(e.ToString());
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            if (!condition) Break("assertion failed");
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            if (!condition) Break(message);
        }

        [Conditional("DEBUG")]
        public static void AssertNotNull(object obj, string message)
        {
            Assert(obj != null, message);
        }
    }
}