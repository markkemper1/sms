using Sms.Messaging;

namespace Sms.Aws
{
    public class AwsSqsFactory : IMessagingFactory
    {
        public const string ProviderName = "awssqs";

        public string Name { get { return ProviderName; } }

        public IMessageSink Sender(string queueName)
        {
            return new AwsSqsMessageSink(ProviderName, queueName);
        }

        public IReceiver Receiver(string queueName)
        {
            return new AwsSqsMessageReceiver(ProviderName, queueName);
        }
    }
}