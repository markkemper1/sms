using System;
using System.Messaging;
using System.Threading;
using Sms.Messaging;

namespace Sms.Msmq
{
    public class MsmqMessageSink : IMessageSink, IDisposable
    {
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

            
        }

        public void Dispose()
        {
        }

        public void Send(SmsMessage smsMessage)
        {
	        using (var messageQueue = new MessageQueue(msmqQueueName))
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
					        throw new Exception("Failed to send message after " + tryNo + " tries", ex);
			        }
			        Thread.Sleep(1000*tryNo*tryNo);
			        tryNo++;
		        }
	        }
        }
    }
}