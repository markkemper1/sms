using Sms.Messaging;

namespace Sms.Test
{
    public class TestFactory : IMessagingFactory
    {
        public string Name { get { return "test"; } }

        public IMessageSink Sender(string queueName)
        {
            return null;
        }

        public IReceiver Receiver(string url)
        {
            return new TestReceiver();
        }
    }
}