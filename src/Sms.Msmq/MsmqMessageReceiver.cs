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


        public ReceivedMessage Receive(TimeSpan? timeout = null)
        {
            var transaction = new MessageQueueTransaction();

            transaction.Begin();
            try
            {
                using (
                    var raw = timeout.HasValue
                                  ? messageQueue.Receive(timeout.Value, transaction)
                                  : messageQueue.Receive(transaction))
                {




                    var message = ((MsmqMessage) raw.Body).ToMessage();

                    Func<MessageQueueTransaction, Action<bool>> onRecieve = queueTransaction =>
                        {
                            Action<bool> recieveHandler = x =>
                                {
                                    if (x)
                                        queueTransaction.Commit();
                                    else
                                        queueTransaction.Abort();
                                };
                            return recieveHandler;
                        };

                    return new ReceivedMessage(message, onRecieve(transaction));
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