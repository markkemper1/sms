using System;
using Sms.Messaging;

namespace Sms.Test
{
    public class TestReciever : IReciever<SmsMessage>
    {
        public void Subscribe(Func<SmsMessage, bool> action)
        {
        }

        public void Dispose()
        {
        }

        public Message<SmsMessage> Receive(TimeSpan? timeout = null)
        {
            return null;
        }

       
    }
}
