using System;
using Sms.Messaging;

namespace Sms.Test
{
    public class TestReceiver : IReceiver<SmsMessage>
    {
        public void Subscribe(Func<SmsMessage, bool> action)
        {
        }

        public void Dispose()
        {
        }

        public string QueueName { get { return "TestReceiver"; } }

        public Message<SmsMessage> Receive(TimeSpan? timeout = null)
        {
            return null;
        }

       
    }
}
