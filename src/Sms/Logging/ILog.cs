using System.Collections.Generic;

namespace Sms.Logging
{
    public interface ILog
    {
        bool IsEnabled(LogLevel level);
        void Log(LogLevel level, string message, IDictionary<string, object> properties = null);
    }

    public enum LogLevel : int
    {
        Verbose =0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Exception = 5,
        Fatal = 6
    }
}