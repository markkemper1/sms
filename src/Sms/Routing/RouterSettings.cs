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
                return Config.Require("Router.ProviderName").Value;
            }
        }
        
        public static string SendQueueName
        {
            get { return Config.Setting("Router.SendQueueName", sendMessageQueueName).Value; }
        }

        public static string NextMessageQueueName
        {
            get { return Config.Setting("Router.NextMessageQueueName", nextMessageQueueName).Value; }
        }

        public static string ServiceNameHeaderKey
        {
            get { return Config.Setting("Router.ServiceNameHeaderKey", serviceNameHeaderKey).Value; }
        }

    }
}