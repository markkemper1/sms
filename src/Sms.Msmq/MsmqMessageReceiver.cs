using System;
using System.Messaging;
using Sms.Messaging;

namespace Sms.Msmq
{
    public class MsmqMessageReceiver : IMessageReciever, IDisposable
    {
        readonly MessageQueue messageQueue = null;
        //private bool stopping;
        private readonly string queueName;

        //public bool Receiving { get; private set; }

        public MsmqMessageReceiver(string queueName)
        {
            this.queueName = @".\Private$\" + queueName;

            EnsureQueueExists.OfName(this.queueName);

            messageQueue = new MessageQueue(this.queueName);
            messageQueue.Formatter = new XmlMessageFormatter(new Type[1] { typeof(MsmqMessage) });
        }

        public void Dispose()
        {
            //this.Stop();

            //int times = 3;

            //while (times > 0)
            //{
            //    if(!Receiving)
            //        break;

            //    Thread.Sleep(700);
            //    times--;
            //}

            messageQueue.Dispose();
        }

        private MessageQueueTransaction transaction = null;

        public ReceivedMessage Receive(TimeSpan? timeout = null)
        {
            if (transaction != null)
                throw new ArgumentException(
                    "You are already processing a message, please called Remove or Add back to Queue on the message before receiving another message");

            transaction = new MessageQueueTransaction();

            transaction.Begin();
            try
            {
                using (
                    var raw = timeout.HasValue
                                  ? messageQueue.Receive(timeout.Value, transaction)
                                  : messageQueue.Receive(transaction))
                {




                    var message = ((MsmqMessage) raw.Body).ToMessage();

                    return new ReceivedMessage(message, success =>
                        {
                            if (success)
                                transaction.Commit();
                            else
                                transaction.Abort();

                            transaction = null;

                        });
                }
            }
            catch (MessageQueueException ex)
            {
                if (ex.Message == "Timeout for the requested operation has expired.")
                {
                    transaction.Abort();
                    transaction = null;
                    return null;
                }

                throw;
            }
        }

        //public void Subscribe(Func<SmsMessage, bool> action)
        //{
        //    Receiving = true;
        //    stopping = false;

        //    try
        //    {
        //        while (!stopping)
        //        {
        //            try
        //            {
                       
        //            }
        //            catch (MessageQueueException ex)
        //            {
        //                if (ex.Message == "Timeout for the requested operation has expired.")
        //                {
        //                    Thread.Sleep(500);
        //                    continue;
        //                }

        //                throw;
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        Receiving = false;
        //    }
        //}

        //public void Stop()
        //{
        //    stopping = true;
        //}
    }
}