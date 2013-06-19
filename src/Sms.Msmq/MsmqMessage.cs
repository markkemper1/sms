using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack.Text;
using Sms.Messaging;

namespace Sms.Msmq
{
    [Serializable]
    public class MsmqMessage
    {
        public static MsmqMessage Create(SmsMessage m)
        {
            return new MsmqMessage()
                {
                    Body = m.Body,
                    Id = m.Id,
                    To = m.ToAddress,
                    HeaderKeys = m.Headers.Keys.ToArray(),
                    HeaderValues = m.Headers.Values.ToArray()
                };
        }

        public string Id { get; set; }

        public string To { get; set; }

        public string[] HeaderKeys { get; set; }

        public string[] HeaderValues { get; set; }

        public string Body { get;  set; }

        public SmsMessage ToMessage()
        {
            var dictionary = new Dictionary<string, string>();

            for (int i = 0; i < HeaderKeys.Length; i++)
            {
                dictionary.Add(HeaderKeys[i], HeaderValues[i]);
            }
            return new SmsMessage(To, Body, dictionary);
        }
    }


}
