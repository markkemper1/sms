using System;
using System.Collections.Generic;
using System.Linq;

namespace Sms.Logging
{
    public class ConsoleLogger : ILog
    {
        private readonly LogLevel logLevelEnabled;

        public ConsoleLogger(LogLevel logLevel)
        {
            this.logLevelEnabled = logLevel;
        }


        public bool IsEnabled(LogLevel level)
        {
            return level >= logLevelEnabled;
        }

        public void Log(LogLevel level, string message, IDictionary<string, object> properties = null)
        {
            if (!IsEnabled(level)) return;

            DateTime? timeStamp = properties.TimeStamp();
            var dateString = timeStamp.HasValue ? timeStamp.Value.ToString("HH:mm") + " - " : null;

            var levelString = level.ToString();

            var loggerName = properties.LoggerName() != null ? " - " + properties.LoggerName() : null;

            Console.WriteLine("{0}{1}{2}{3}", dateString, levelString, message, loggerName);
        }
    }
}