using System;
using Sms.Messaging;

namespace Sms.Test
{
    public class TestReciever : IMessageReciever
    {
        public void Subscribe(Func<SmsMessage, bool> action)
        {
        }

        public void Dispose()
        {
        }

        public Result<SmsMessage> Receive(TimeSpan? timeout = null)
        {
            return null;
        }

       
    }
}
