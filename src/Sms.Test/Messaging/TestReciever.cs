using System;
using Sms.Messaging;

namespace Sms.Test
{
    public class TestReceiver : IReceiver
    {
        public void Subscribe(Func<SmsMessage, bool> action)
        {
        }

        public void Dispose()
        {
        }

        public string ProviderName { get; private set; }
        public string QueueName { get { return "TestReceiver"; } }

        public MessageResult Receive(TimeSpan? timeout = null)
        {
            return null;
        }

       
    }
}
