using System;
using System.Collections.Generic;

namespace Sms.Messaging
{
    public class SmsMessage
    {
        private IDictionary<string, string> headers;
 
        public SmsMessage(string toAddress, string body, IDictionary<string, string> headers = null)
        {
            if (toAddress == null) throw new ArgumentNullException("toAddress");
            Id = Guid.NewGuid().ToString();
            ToAddress = toAddress;
            Body = body;
            this.headers = headers ?? new Dictionary<string, string>();
        }

        public string Id { get; private set; }

        public IDictionary<string,string> Headers { get { return new Dictionary<string, string>(headers);} }

        public string ToAddress { get; set; }
        public string Body { get; private set; }
    }

    public class ReceivedMessage : SmsMessage
    {
        private readonly SmsMessage message;
        private readonly Action<bool> processed;

        public ReceivedMessage(SmsMessage message, Action<bool> processed) : base(message.ToAddress, message.Body, message.Headers)
        {
            this.message = message;
            this.processed = processed;
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