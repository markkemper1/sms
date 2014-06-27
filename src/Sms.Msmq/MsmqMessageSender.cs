using System;
using System.Messaging;
using System.Threading;
using Sms.Messaging;

namespace Sms.Msmq
{
    public class MsmqMessageSink : IMessageSink, IDisposable
    {
        readonly MessageQueue messageQueue;
        private readonly string msmqQueueName;
        public string QueueName { get; private set; }
        public string ProviderName { get; private set; }

        public MsmqMessageSink(string providerName, string queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");


            QueueName = queueName;
            ProviderName = providerName;

            msmqQueueName = @".\Private$\" + queueName;

            EnsureQueueExists.OfName(msmqQueueName);

            messageQueue = new MessageQueue(msmqQueueName);
        }

        public void Dispose()
        {
            messageQueue.Dispose();
        }

        public void Send(SmsMessage smsMessage)
        {
            int tryNo = 1;
            var m = SmsMessageContent.Create(smsMessage);

            while (tryNo < 4)
            {
                try
                {
                    using (var transaction = new MessageQueueTransaction())
                    {
                        tryNo++;
                        transaction.Begin();
                        messageQueue.Send(m, m.To, transaction);
                        transaction.Commit();
                        return;
                    }
                }
                catch (MessageQueueException ex)
                {
                    Logger.Warn("Msmq sender: error sending: {0}, tried {1} times", ex, tryNo);

                    if (tryNo >= 4)
                        throw new Exception("Failed to send message after "+ tryNo+" tries",  ex);
                }
                Thread.Sleep(1000 * tryNo * tryNo);
                tryNo++;
            }
        }
    }
}