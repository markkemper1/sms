using System;
using System.IO;
using System.Messaging;
using NUnit.Framework;
using Sms.Messaging;

namespace Sms.Msmq.Test
{
    [TestFixture]
    public class MsmqMessageSenderTest 
    {
        [Test]
        public void should_send_test_message()
        {
            const string queueName = @"SomeTestName";
            var sender = new MsmqMessageSender(queueName);

            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));

            var messageQueue = new MessageQueue(@".\Private$\" + queueName);
            var messages = messageQueue.GetAllMessages();

            try
            {
                Assert.That(messages.Length, Is.GreaterThan(0));

                Console.WriteLine(messages.Length);

                using (var streamReader = new StreamReader(messages[0].BodyStream))
                {
                    Console.WriteLine(streamReader.ReadToEnd());
                }
            }
            finally
            {
                messageQueue.Purge();
                sender.Dispose();
            }
            // after all processing, delete all the messages
           
        }
    }
}
