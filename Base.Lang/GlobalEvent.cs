using System;
using System.Diagnostics;
using System.Reactive.Subjects;

namespace Base.Lang
{
    public abstract class GlobalEvent
    {
        public static readonly Subject<GlobalEvent> Emitter = new Subject<GlobalEvent>();

        static GlobalEvent()
        {
            var cd = AppDomain.CurrentDomain;
            cd.ProcessExit += ProcessExitHandler;
            cd.UnhandledException +=
                (s, e) => OnUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        public static T Fire<T>(T e) where T : GlobalEvent
        {
            Emitter.OnNext(e);
            return e;
        }

        public sealed class ProcessExit : GlobalEvent
        {
            public static readonly ProcessExit Instance = new ProcessExit();

            private ProcessExit()
            {
            }
        }

        public sealed class UnhandledException : GlobalEvent
        {
            public readonly Exception Exception;
            internal UnhandledException(Exception exception)
            {
                Exception = exception;
            }
        }

        public static void FireProcessExit()
        {
            try
            {
                Trace.WriteLine("CurrentDomain.ProcessExit");
                Fire(ProcessExit.Instance);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CurrentDomain.ProcessExit() " + ex);
            }
        }

        private static void ProcessExitHandler(object s, EventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit -= ProcessExitHandler;
            FireProcessExit();
        }

        private static void OnUnhandledException(Exception e, bool terminating)
        {
            if (e != null) 
            try
            {
                Bugs.Break("Unhandled exception " + e.ToString());
                Fire(new UnhandledException(e));
            }
            catch (Exception ex)
            {
                try
                {
                    Bugs.Break("while handling unhandled exception " + ex);
                }
                catch
                {
                }
            }

            if (terminating)
                FireProcessExit();
        }
    }
}