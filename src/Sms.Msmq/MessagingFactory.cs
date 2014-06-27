using System;
using Sms.Messaging;

namespace Sms.Msmq
{
    public class MsmqFactory : IMessagingFactory
    {
        public const string ProviderName = "msmq";

        public string Name { get { return ProviderName; } }

        public IMessageSink Sender(string queueName)
        {
            return new MsmqMessageSink(ProviderName,queueName);
        }

        public IReceiver Receiver(string queueName)
        {
            return new MsmqMessageReceiver(ProviderName, queueName);
        }
    }
}