using System;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.SQS;
using Amazon.SQS.Model;
using Sms.Messaging;

namespace Sms.Aws
{
    public class AwsSqsMessageSink : IMessageSink, IDisposable
    {
        private AmazonSQSClient client;
        private string queueUrl { get; set; }

        public string QueueName { get; private set; }
        public string ProviderName { get; private set; }

        public AwsSqsMessageSink(string provideName, string queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");

            QueueName = queueName;
            ProviderName = provideName;

            client = ClientFactory.Create();
            queueUrl = client.GetQueueUrl(queueName);
        }

        public void Send(SmsMessage smsMessage)
        {
            int tryNo = 1;
            var m = SmsMessageContent.Create(smsMessage);

	        var payload = SmsMessageContent.Serialization.ToBase64(m);

            while (tryNo < 4)
            {

                var sendRequest = new SendMessageRequest();

	            sendRequest.MessageBody = payload;
                sendRequest.QueueUrl = queueUrl;

                sendRequest.MessageAttributes.Add(Config.ToAttributename, new MessageAttributeValue()
                {
                    DataType = "String",
                    StringValue = m.To
                });

				sendRequest.MessageAttributes.Add(Config.ContentType, new MessageAttributeValue()
				{
					DataType = "String",
					StringValue = Config.ContentTypeBase64EncodedSmsMessageContent
				});

                try
                {

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