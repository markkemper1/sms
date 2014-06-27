using System;
using System.Messaging;
using System.Threading;
using Sms.Messaging;

namespace Sms.Msmq
{
    public class MsmqMessageReceiver : IReceiver, IDisposable
    {
        readonly MessageQueue messageQueue = null;
        //private bool stopping;
        private readonly string queueName;

        //public bool Receiving { get; private set; }

        public string ProviderName { get; private set; }

        public MsmqMessageReceiver(string providerName, string queueName)
        {
            ProviderName = providerName;
            QueueName = queueName;
            this.queueName = @".\Private$\" + queueName;

            EnsureQueueExists.OfName(this.queueName);

            
            messageQueue = new MessageQueue(this.queueName);
            messageQueue.Formatter = new XmlMessageFormatter(new Type[1] { typeof(SmsMessageContent) });
        }

        public void Dispose()
        {
            messageQueue.Dispose();
        }

        public string QueueName { get; private set; }

        public MessageResult Receive(TimeSpan? timeout = null)
        {
            MessageQueueTransaction transaction = null;
            try
            {
                try
                {
                    transaction = new MessageQueueTransaction();
                    transaction.Begin();
                    using (
                        var raw = timeout.HasValue
                                      ? messageQueue.Receive(timeout.Value, transaction)
                                      : messageQueue.Receive(transaction))
                    {

                        var message = ((SmsMessageContent) raw.Body).ToMessage();

                        Func<MessageQueueTransaction, Action<bool>> onReceive = queueTransaction =>
                            {
                                Action<bool> handler = x =>
                                    {
                                        if (queueTransaction.Status == MessageQueueTransactionStatus.Pending)
                                        {
                                            if (x)
                                                queueTransaction.Commit();
                                            else
                                                queueTransaction.Abort();
                                        }

                                        queueTransaction.Dispose();
                                    };
                                return handler;
                            };

                        return new MessageResult(message, onReceive(transaction));
                    }
                }
                catch (MessageQueueException ex)
                {
                    TryToAbortTransaction(transaction);

                    if (ex.Message == "Timeout for the requested operation has expired.")
                    {
                        Logger.Debug("Msmq receiver: timeout while waiting for message (this is ok)");
                        return null;
                    }
                    int sleepForMs = timeout.HasValue ? timeout.Value.Milliseconds : 500;
                    Logger.Warn("Msmq receiver: message queue exception on receive, sleeping for {0} Details: {1}",
                                sleepForMs, ex);
                    Thread.Sleep(sleepForMs);
                    return null;
                }
            }
            catch (Exception ex)
            {
                TryToAbortTransaction(transaction);
                Logger.Warn("Msmq receiver: unknow error: {0}", ex);
                throw;
            }
        }

        private void TryToAbortTransaction(MessageQueueTransaction transaction)
        {
            if (transaction != null)
            {
                try
                {
                    transaction.Abort();
                    transaction.Dispose();
                    transaction = null;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Msmq receiver: error shutting down transaction: {0}", ex);
                }
            }
        }

    }
}