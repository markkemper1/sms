using System;
using Sms.Messaging;

namespace Sms.Msmq
{
    public class MsmqFactory : IMessagingFactory
    {
        public const string ProviderName = "msmq";

        public string Name { get { return ProviderName; } }

        public IMessageSender Sender(string queueName)
        {
            return new MsmqMessageSender(queueName);
        }

        public IMessageReciever Reciever(string queueName)
        {
            return new MsmqMessageReceiver(queueName);
        }
    }
}