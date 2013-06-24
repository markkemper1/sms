using System;
using System.Collections.Generic;
using System.Linq;

namespace Sms.Messaging
{
    [Serializable]
    public class SmsMessageContent
    {
        public static SmsMessageContent Create(SmsMessage m)
        {
            return new SmsMessageContent()
            {
                Body = m.Body,
                To = m.ToAddress,
                HeaderKeys = m.Headers.Keys.ToArray(),
                HeaderValues = m.Headers.Values.ToArray()
            };
        }

        public string To { get; set; }

        public string[] HeaderKeys { get; set; }

        public string[] HeaderValues { get; set; }

        public string Body { get; set; }

        public SmsMessage ToMessage()
        {
            if (String.IsNullOrWhiteSpace(To)) throw new InvalidOperationException("Cannot create SmsMessage with null or empty To property");

            var dictionary = new Dictionary<string, string>();

            for (int i = 0; i < HeaderKeys.Length; i++)
            {
                dictionary.Add(HeaderKeys[i], HeaderValues[i]);
            }
            return new SmsMessage(To, Body, dictionary);
        }
    }
}
