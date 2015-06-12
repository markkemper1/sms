using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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


		public static class Serialization
	    {
		    public static string ToBase64(SmsMessageContent content)
		    {
				return Convert.ToBase64String(ToBytes(content));
		    }

			public static byte[] ToBytes(SmsMessageContent content)
		    {
			    using (var ms = new MemoryStream())
			    {
				    ToStream(content, ms);
				    return ms.ToArray();
			    }
		    }

			public static void ToStream(SmsMessageContent content, Stream stream)
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, content);
			}

			public static SmsMessageContent FromBase64(string base64Content)
			{
				return FromBytes(Convert.FromBase64String(base64Content));
			}

			public static SmsMessageContent FromBytes(byte[] bytes)
			{
				using (var ms = new MemoryStream())
				{
					ms.Write(bytes, 0, bytes.Length);
					ms.Position = 0;
					return FromStream(ms);
				}
			}

			public static SmsMessageContent FromStream(Stream stream)
			{
				var formatter = new BinaryFormatter();
				return (SmsMessageContent)formatter.Deserialize(stream);
			}
			
	    }
    }
}
