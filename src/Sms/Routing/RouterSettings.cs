namespace Sms.Routing
{
    public static class RouterSettings
    {
        private const string nextMessageQueueName = "receiveNextMessage";
        private const string sendMessageQueueName = "sendMessage";
        private const string serviceNameHeaderKey = "broker.serviceName";

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

        public static string NextMessageQueueName
        {
            get { return Config.Setting("Sms.Router.NextMessageQueueName", nextMessageQueueName).Value; }
        }

        public static string ServiceNameHeaderKey
        {
            get { return Config.Setting("Sms.Router.ServiceNameHeaderKey", serviceNameHeaderKey).Value; }
        }

    }
}