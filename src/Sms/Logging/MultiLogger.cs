using System;
using System.Collections.Generic;
using System.Linq;

namespace Sms.Logging
{
    internal class MultiLogger : ILog
    {
        private readonly string loggerName;
        private readonly IEnumerable<ILog> logs;

        public MultiLogger(IEnumerable<Func<ILog>> logs, string loggerName = null)
        {
            this.loggerName = loggerName;
            this.logs = logs.Select(x=>x());
        }

        public void Log(string message, IDictionary<string, object> properties = null)
        {
         
        }

        public bool IsEnabled(LogLevel level)
        {
            return true;
        }

        public void Log(LogLevel level, string message, IDictionary<string, object> properties = null)
        {
            properties = new Dictionary<string, object>();

            if (loggerName != null)
            {
                properties["_LoggerName"] = loggerName;
            }

            properties["_TimeStamp"] = DateTime.UtcNow;

            foreach (var log in logs)
            {
                if(log.IsEnabled(level))
                    log.Log(level, message, properties);
            }
        }
    }
}