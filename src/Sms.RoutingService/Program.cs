using System;
using System.IO;
using log4net;
using log4net.Config;
using Sms.Router;

namespace Sms.RoutingService
{
     class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RouterService));
        private static readonly ILog smsLog = LogManager.GetLogger("SMS");

        static void Main(string[] args)
        {
            SetUpLogging();

            var router = new RouterService();
            router.Start();

            Console.WriteLine("Press the any key to exit");
            Console.ReadKey();

            router.Stop();

            Console.WriteLine("Done");
        }


        private static void SetUpLogging()
        {
            var configFile = new FileInfo("log4net.config");

            if (!configFile.Exists)
                throw new ApplicationException("Log4net config file doesn't exist!. No you can't run without it!");

            XmlConfigurator.ConfigureAndWatch(configFile);

            log.Info("RouterService logging setup..");

            Logger.Setup.Info(s => smsLog.Info(s));
            Logger.Setup.Warn(s => smsLog.Warn(s));
            Logger.Setup.Error(s => smsLog.Error(s));
            Logger.Setup.Fatal(s => smsLog.Fatal(s));
        }
    }
}
