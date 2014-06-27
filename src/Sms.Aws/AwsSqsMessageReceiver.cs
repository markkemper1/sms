using System;
using System.Collections.Generic;
using System.Net;
using Amazon.SQS;
using Amazon.SQS.Model;
using Sms.Messaging;

namespace Sms.Aws
{
    public class AwsSqsMessageReceiver : IReceiver, IDisposable
    {
        private readonly string queueName;
        private AmazonSQSClient client;
        private string queueUrl;

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
                    MessageAttributeNames = new List<string> {Config.ToAttributename, Config.HeaderKeysAttributename, Config.HeaderValuesAttributename}
                };

                var raw = client.ReceiveMessage(request);


                if (raw.HttpStatusCode != HttpStatusCode.OK)
                    throw new WebException("Failed to get a message, status code: " + raw.HttpStatusCode);

                if (raw.Messages.Count == 0) return null;


                var awsMessage = raw.Messages[0];

                if (!awsMessage.MessageAttributes.ContainsKey(Config.ToAttributename))
                    throw new InvalidOperationException("The Message is missing the To Attribute");

                var message = new SmsMessageContent
                {
                    To = awsMessage.MessageAttributes[Config.HeaderKeysAttributename].StringValue,
                    Body = awsMessage.Body,
                    HeaderKeys = awsMessage.MessageAttributes.ContainsKey(Config.HeaderKeysAttributename) ? awsMessage.MessageAttributes[Config.HeaderKeysAttributename].StringListValues.ToArray() : null,
                    HeaderValues = awsMessage.MessageAttributes.ContainsKey(Config.HeaderValuesAttributename) ? awsMessage.MessageAttributes[Config.HeaderValuesAttributename].StringListValues.ToArray() : null
                }.ToMessage();


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