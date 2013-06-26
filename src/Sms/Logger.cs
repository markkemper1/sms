using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms
{
    public static class Logger
    {
        private enum Level
        {
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }

        public class SetupHelpler
        {
            internal SetupHelpler(){}

            public SetupHelpler Debug(Action<string> value) { Logger.DebugLogger = value; return this; }
            public SetupHelpler Info(Action<string> value) { Logger.InfoLogger = value; return this; }
            public SetupHelpler Warn(Action<string> value) { Logger.WarnLogger = value; return this; }
            public SetupHelpler Error(Action<string> value) { Logger.ErrorLogger = value; return this; }
            public SetupHelpler Fatal(Action<string> value) { Logger.FatalLogger = value; return this; }
        }

        private static Action<string> DebugLogger = null;
        private static Action<string> InfoLogger = x => System.Diagnostics.Trace.WriteLine(x);
        private static Action<string> WarnLogger = x => System.Diagnostics.Trace.WriteLine(x);
        private static Action<string> ErrorLogger = x => System.Diagnostics.Trace.WriteLine(x);
        private static Action<string> FatalLogger = x => System.Diagnostics.Trace.WriteLine(x);

        public static readonly SetupHelpler Setup = new SetupHelpler();

        public static void Debug(string message, params object[] args)
        {
            Log(DebugLogger, message, args);
        }

        public static void Info(string message, params object[] args)
        {
            Log(InfoLogger, message, args);
        }

        public static void Warn(string message, params object[] args)
        {
            Log(WarnLogger, message, args);
        }

        public static void Error(string message, params object[] args)
        {
            Log(ErrorLogger, message, args);
        }

        public static void Fatal(string message, params object[] args)
        {
            Log(FatalLogger, message, args);
        }

        private static void Log(Action<string> logger, string messsage, params object[] args)
        {
            if (logger == null) return;

            if (args.Length == 0)
                logger(messsage);
            else
                logger(String.Format(messsage, args));
        }


    }
}
