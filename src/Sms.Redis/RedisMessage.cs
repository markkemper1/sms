using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack.Text;
using Sms.Messaging;

namespace Sms.Redis
{
    internal class RedisMessage
    {
        public static RedisMessage Create(SmsMessage m)
        {
            return new RedisMessage()
                {
                    Body = m.Body,
                    Headers = m.Headers,
                    Id = m.Id,
                    To = m.ToAddress
                };
        }

        public DateTime ProcessingStartedUtc { get; set; }

        public string Id { get; set; }

        public string To { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public string Body { get;  set; }

        public byte[] Serialize()
        {
            return Serialize(this);
        }

        public static byte[] Serialize(RedisMessage message)
        {
            using (var ms = new MemoryStream())
            {
                TypeSerializer.SerializeToStream(message, ms);
                return ms.ToArray();
            }
        }

        public static RedisMessage DeSerialize(byte[] message)
        {
            using (var ms = new MemoryStream(message))
            {
                return TypeSerializer.DeserializeFromStream<RedisMessage>(ms);
            }
        }

        public SmsMessage ToMessage()
        {
            return new SmsMessage(To,Body,Headers);
        }
    }


}
