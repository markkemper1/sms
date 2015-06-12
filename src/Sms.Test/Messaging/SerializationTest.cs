using System;
using NUnit.Framework;
using Sms.Messaging;

namespace Sms.Test.Messaging
{
	[TestFixture]
	public class SerializationTest
    {
		
		[Test]
		public void CanRoundTripMessage()
		{
			var source = new SmsMessageContent
			{
				Body = Guid.NewGuid().ToString(),
				To = Guid.NewGuid().ToString()
			};
			source.HeaderKeys = new string[10];
			source.HeaderValues = new string[10];
			for (int i = 0; i < 10; i++)
			{
				source.HeaderKeys[i] = Guid.NewGuid().ToString();
				source.HeaderValues[i] = Guid.NewGuid().ToString();
			}

			var base64 = SmsMessageContent.Serialization.ToBase64(source);
			Console.WriteLine(base64);
			Console.WriteLine(base64.Length);

			var result = SmsMessageContent.Serialization.FromBase64(base64);
			
			Assert.AreEqual(source.To, result.To);
			Assert.AreEqual(source.Body, result.Body);

			for (int i = 0; i < 10; i++)
			{
				Assert.AreEqual(source.HeaderKeys[i], result.HeaderKeys[i]);
				Assert.AreEqual(source.HeaderValues[i], result.HeaderValues[i]);
			}


		}

       
    }
}
