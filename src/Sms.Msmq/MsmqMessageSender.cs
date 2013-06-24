using System;
using System.Messaging;
using Sms.Messaging;

namespace Sms.Msmq
{
    public class MsmqMessageSender : IMessageSender<SmsMessage>, IDisposable
    {
        readonly MessageQueue messageQueue;

        private string QueueName { get; set; }

        public MsmqMessageSender(string queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");


            QueueName = @".\Private$\" + queueName;

            EnsureQueueExists.OfName(QueueName);

            messageQueue = new MessageQueue(QueueName);
        }

        public void Dispose()
        {
            messageQueue.Dispose();
        }

        public void Send(SmsMessage smsMessage)
        {
            var m = SmsMessageContent.Create(smsMessage);
            using (var transaction = new MessageQueueTransaction())
            {
                transaction.Begin();
                messageQueue.Send(m, m.To, transaction);
                transaction.Commit();
            }
        }
    }
}