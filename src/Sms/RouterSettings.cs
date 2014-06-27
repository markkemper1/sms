using Sms.Internals;

namespace Sms.Routing
{
    public static class RouterSettings
    {
        private const string sendMessageQueueName = "sms-router-sendMessage";
        private const string serviceNameHeaderKey = "sms-router-serviceName";
        private const string sendErrorQueueName = "sms-router-sendErrors";

        public static string ProviderName {
            get
            {
                return Config.Require("Sms.Router.ProviderName").Value;
            }
        }
        
        public static string SendQueueName
        {
            get { return Config.Setting("Sms.Router.SendQueueName", sendMessageQueueName).Value; }
        }

        public static string ServiceNameHeaderKey
        {
            get { return Config.Setting("Sms.Router.ServiceNameHeaderKey", serviceNameHeaderKey).Value; }
        }

        public static string SendErrorQueueName
        {
            get { return Config.Setting("Sms.Router.SendErrorQueueName", sendErrorQueueName).Value; }
        }
    }
}