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

                var queue = MessageQueue.Create(queueName, true);

                var user = Config.Setting("Sms.Msmq.DefaultQueueUser", "Everyone").Value;

                queue.SetPermissions(user, MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
                queue.SetPermissions("Administrator", MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
            }
        }
    }
}