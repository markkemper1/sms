using Sms.Messaging;

namespace Sms.Test
{
    public class TestFactory : IMessagingFactory
    {
        public string Name { get { return "test"; } }

        public IMessageSender<SmsMessage> Sender(string queueName)
        {
            return null;
        }

        public IReciever<SmsMessage> Reciever(string url)
        {
            return new TestReciever();
        }
    }
}