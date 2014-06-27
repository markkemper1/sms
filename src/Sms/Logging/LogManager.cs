using System;
using System.Collections.Generic;

namespace Sms.Logging
{
    public class LogManager
    {
        private static readonly List<Func<ILog>> loggers = new List<Func<ILog>>()
        {
            () => new ConsoleLogger(LogLevel.Info) 
        };

        public void ClearLoggers()
        {
            lock (loggers)
            {
                loggers.Clear();
            }
        }

        public void AppendLogger(Func<ILog> log)
        {
            lock (loggers)
            {
                loggers.Add(log);
            }
        }

        public static ILog Get(string name = null)
        {
            return new MultiLogger(loggers, name);
        }

        public static ILog Get<T>()
        {
            return Get(typeof (T).Name);
        }
    }
}