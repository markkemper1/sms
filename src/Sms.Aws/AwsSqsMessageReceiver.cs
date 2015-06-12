using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Amazon.SQS;
using Amazon.SQS.Model;
using Sms.Messaging;

namespace Sms.Aws
{
    public class AwsSqsMessageReceiver : IReceiver, IDisposable
    {
        private AmazonSQSClient client;
        private readonly string queueUrl;

        //public bool Receiving { get; private set; }

        public string QueueName { get; private set; }
        public string ProviderName { get; private set; }

        public AwsSqsMessageReceiver(string provideName, string queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");

            QueueName = queueName;
            ProviderName = provideName;

            client = ClientFactory.Create();
            queueUrl = client.GetQueueUrl(queueName);
        }

        public MessageResult Receive(TimeSpan? timeout = null)
        {
            try
            {
                var request = new ReceiveMessageRequest()
                {
                    MaxNumberOfMessages = 1,
                    QueueUrl = queueUrl,
                    WaitTimeSeconds = timeout.HasValue ? (int) timeout.Value.TotalSeconds : 0,
                    MessageAttributeNames = new List<string>{"All"}
                };

                var raw = client.ReceiveMessage(request);


                if (raw.HttpStatusCode != HttpStatusCode.OK)
                    throw new WebException("Failed to get a message, status code: " + raw.HttpStatusCode);

                if (raw.Messages.Count == 0) return null;


                var awsMessage = raw.Messages[0];

                if (!awsMessage.MessageAttributes.ContainsKey(Config.ToAttributename))
                    throw new InvalidOperationException("The Message is missing the To Attribute");


				SmsMessage message = null;
	            if (awsMessage.MessageAttributes.ContainsKey(Config.ContentType)
					&& awsMessage.MessageAttributes[Config.ContentType].StringValue ==
					Config.ContentTypeBase64EncodedSmsMessageContent
		            )
	            {
					message = SmsMessageContent.Serialization.FromBase64(awsMessage.Body)
						.ToMessage();
	            }
	            else
	            {
		            var headerKeys = new List<string>();
		            var headerValues = new List<string>();

		            for (int i = 0; i < 100; i++)
		            {
			            var key = Config.HeaderKeysAttributename + i;
			            var valueKey = Config.HeaderValuesAttributename + i;

			            if (!awsMessage.MessageAttributes.ContainsKey(key))
				            break;

			            headerKeys.Add(awsMessage.MessageAttributes[key].StringValue);
			            headerValues.Add(awsMessage.MessageAttributes[valueKey].StringValue);

		            }

		            var body = Encoding.UTF8.GetString(Convert.FromBase64String(awsMessage.Body));

		            message = new SmsMessageContent
		            {
			            To = awsMessage.MessageAttributes[Config.ToAttributename].StringValue,
			            Body = body,
			            HeaderKeys = headerKeys.ToArray(),
			            HeaderValues = headerValues.ToArray()
		            }.ToMessage();

	            }

	            Func<string, Action<bool>> onReceive = receiptHandle =>
                {
                    Action<bool> handler = x =>
                    {
                        if (x)
                            DeleteMessage(receiptHandle);
                        else
                            SetMessageVisible(receiptHandle);
                    };
                    return handler;
                };

                return new MessageResult(message, onReceive(awsMessage.ReceiptHandle));

            }
            catch (Exception ex)
            {
                Logger.Error("Aws.Sqs receiver: Error:" + ex.ToString(), ex);
                return null;
            }
        }

        private void SetMessageVisible(string receiptHandle)
        {
            if (client == null)
                throw new InvalidOperationException("The client has been disposed!");

            client.ChangeMessageVisibility(new ChangeMessageVisibilityRequest()
            {
                QueueUrl = queueUrl,
                ReceiptHandle = receiptHandle,
                VisibilityTimeout = 0
            });
        }

        private void DeleteMessage(string receiptHandle)
        {
            if (client == null)
                throw new InvalidOperationException("The client has been disposed!");

            client.DeleteMessage(new DeleteMessageRequest {
                QueueUrl = queueUrl,
                ReceiptHandle = receiptHandle
            });
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