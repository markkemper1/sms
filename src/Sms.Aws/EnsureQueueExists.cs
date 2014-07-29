using System;
using System.Collections.Generic;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace Sms.Aws
{
    public static class EnsureQueueExists
    {
        private static Dictionary<string, string> queueUrls = new Dictionary<string, string>();
        static readonly object lockMe = new object();

        public static string GetQueueUrl(this AmazonSQSClient client, string queueName)
        {
            if (queueUrls.ContainsKey(queueName))
                return queueUrls[queueName];

            lock (lockMe)
            {
                if (queueUrls.ContainsKey(queueName))
                    return queueUrls[queueName];

            var response = client.CreateQueue(new CreateQueueRequest()
                {
                    QueueName = queueName
                });

                if (response == null || String.IsNullOrWhiteSpace(response.QueueUrl))
                    throw new ApplicationException("Failed to get the queue url for queue: " + queueName);

                queueUrls[queueName] = response.QueueUrl;

                return response.QueueUrl;
            }
        }
    }
}