using System;
using Sms.Messaging;

namespace Sms
{
    public class MessageResult
    {
        private readonly SmsMessage message;
        private readonly Action<bool> processed;

        public MessageResult(SmsMessage message, Action<bool> processed) 
        {
            this.message = message;
            this.processed = processed;
        }
        public SmsMessage Item { get { return message; } }

        public void Processed(bool successful)
        {
            processed(successful);
        }

        public void Success()
        {
            processed(true);
        }

        public void Failed()
        {
            processed(false);
        }
    }
}