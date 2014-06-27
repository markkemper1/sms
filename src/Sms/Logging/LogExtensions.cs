using System;
using System.Collections.Generic;

namespace Sms.Logging
{
    public static class LogExtensions
    {
        public static void Debug(this ILog log, string message, IDictionary<string, object> properties = null) { log.Log(LogLevel.Debug, message, properties); }
        public static void Info(this ILog log, string message, IDictionary<string, object> properties = null) { log.Log(LogLevel.Info, message, properties); }
        public static void Warn(this ILog log, string message, IDictionary<string, object> properties = null) { log.Log(LogLevel.Warn, message, properties); }
        public static void Error(this ILog log, string message, IDictionary<string, object> properties = null) { log.Log(LogLevel.Error, message, properties); }
        public static void Error(this ILog log, Exception ex, IDictionary<string, object> properties = null) { log.Log(LogLevel.Error, ex.ToString(), properties); }
        public static void Error(this ILog log, string message, Exception ex, IDictionary<string, object> properties = null) { log.Log(LogLevel.Error, message + "\n" + ex, properties); }
        public static void Exception(this ILog log, string message, IDictionary<string, object> properties = null) { log.Log(LogLevel.Exception, message, properties); }
        public static void Exception(this ILog log, Exception ex, IDictionary<string, object> properties = null) { log.Log(LogLevel.Exception, ex.ToString(), properties); }
        public static void Exception(this ILog log, string message, Exception ex, IDictionary<string, object> properties = null) { log.Log(LogLevel.Fatal, message + "\n" + ex, properties); }
        public static void Fatal(this ILog log, Exception ex, IDictionary<string, object> properties = null) { log.Log(LogLevel.Fatal, ex.ToString(), properties); }
        public static void Fatal(this ILog log, string message, Exception ex, IDictionary<string, object> properties = null) { log.Log(LogLevel.Fatal, message + "\n"+ ex, properties); }
        public static void Fatal(this ILog log, string message, IDictionary<string, object> properties = null) { log.Log(LogLevel.Fatal, message, properties); }

        private static void Log(this ILog log, LogLevel level, string message, IDictionary<string, object> properties = null)
        {
            properties = properties ?? new Dictionary<string, object>();
            log.Log(level, message);
        }

        public static string Level(this IDictionary<string, object> properties)
        {
            return properties.ContainsKey("_LogLevel") ? (string)properties["_LogLevel"] : null;
        }

        public static DateTime? TimeStamp(this IDictionary<string, object> properties)
        {
            return properties.ContainsKey("_TimeStamp") ? (DateTime?)properties["_TimeStamp"] : (DateTime?)null;
        }

        public static string LoggerName(this IDictionary<string, object> properties)
        {
            return properties.ContainsKey("_LoggerName") ? (string) properties["_LoggerName"] : null;
        }
    }

}
