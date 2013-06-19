using System;
using System.Messaging;

namespace Sms.Msmq
{
    public static class EnsureQueueExists
    {
        static readonly object lockMe = new object();

        public static void OfName(string queueName)
        {
            if (MessageQueue.Exists(queueName))
                return;

            lock(lockMe)
            {
                if (MessageQueue.Exists(queueName))
                    return;

                MessageQueue.Create(queueName, true);
            }
        }
    }
}