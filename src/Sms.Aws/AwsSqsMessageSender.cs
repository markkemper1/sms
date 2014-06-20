using System;
using System.Linq;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using Sms.Messaging;

namespace Sms.Aws
{
    public class AwsSqsMessageSender : IMessageSender<SmsMessage>, IDisposable
    {
        private AmazonSQSClient client;
        private string queueUrl { get; set; }

        public AwsSqsMessageSender()
        {

        }

        public AwsSqsMessageSender(string queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");

            client = ClientFactory.Create();
            queueUrl = client.GetQueueUrl(queueName);
        }

        public void Send(SmsMessage smsMessage)
        {
            int tryNo = 1;
            var m = SmsMessageContent.Create(smsMessage);

            while (tryNo < 4)
            {
                try
                {
                    var sendRequest = new SendMessageRequest();

                    sendRequest.MessageBody = m.Body;
                    sendRequest.QueueUrl = queueUrl;

                    sendRequest.MessageAttributes.Add(Config.ToAttributename, new MessageAttributeValue()
                    {
                        DataType = "String",
                        StringValue = m.To
                    });

                    if (m.HeaderKeys.Any())
                    {
                        sendRequest.MessageAttributes.Add(Config.HeaderKeysAttributename, new MessageAttributeValue()
                        {
                            DataType = "String",
                            StringListValues = m.HeaderKeys.ToList()
                        });

                        sendRequest.MessageAttributes.Add(Config.HeaderValuesAttributename, new MessageAttributeValue()
                        {
                            DataType = "String",
                            StringListValues = m.HeaderValues.ToList()
                        });
                    }


                    client.SendMessage(sendRequest);
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Warn("AWS.SQS sender: error sending: {0}, tried {1} times", ex, tryNo);

                    if (tryNo >= 4)
                        throw new Exception("Failed to send message after " + tryNo + " tries", ex);
                }
                Thread.Sleep(1000 * tryNo * tryNo);
                tryNo++;
            }
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }
    }
}