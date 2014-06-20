using System;
using System.Collections.Generic;
using Amazon.SQS.Model;
using NUnit.Framework;
using Sms.Messaging;

namespace Sms.Aws.Test
{
    [TestFixture]
    public class AwsSqsMessageSenderTest
    {
        [Test]
        public void should_send_test_message()
        {
            const string queueName = @"SomeTestName";
            var sender = new AwsSqsMessageSender(queueName);

            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));

            using (var awsClient = ClientFactory.Create())
            {
                var url = awsClient.GetQueueUrl(queueName);

                var response = awsClient.ReceiveMessage(new ReceiveMessageRequest()
                {
                    MaxNumberOfMessages = 1,
                    QueueUrl = url,
                    WaitTimeSeconds = 10,
                    MessageAttributeNames = new List<string> { "To", "HeaderKeys", "HeaderValues" }
                }
                    );

                Console.WriteLine(response.Messages.Count);

                Assert.That(response.Messages.Count, Is.GreaterThan(0));
            }


            // after all processing, delete all the messages

        }
    }
}
