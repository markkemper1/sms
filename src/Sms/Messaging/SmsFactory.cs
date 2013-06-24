using System;
using System.Collections.Generic;
using System.Linq;
using Sms.Internals;

namespace Sms.Messaging
{
    public class SmsFactory
    {
        private Dictionary<string, IMessagingFactory> factories;

        static readonly object LockMe = new object();

        private static readonly SmsFactory Instance = new SmsFactory();

        public static IMessageSender<SmsMessage> Sender(string provider, string queueName)
        {
            return Instance.GetFactory(provider).Sender(queueName);
        }

        public static IReceiver<SmsMessage> Receiver(string provider, string url)
        {
            return Instance.GetFactory(provider).Receiver(url);
        }

        private IMessagingFactory GetFactory(string scheme)
        {
            if (factories == null) CreateFactories();

            if (!factories.ContainsKey(scheme))
                throw new ArgumentException("The protocol is not supported: " + scheme);

            return factories[scheme];
        }

        private void CreateFactories()
        {
            if (factories != null) return;

            lock (LockMe)
            {
                if (factories != null) return;
                factories = GenericFactory.FindAndBuild<IMessagingFactory>().ToDictionary(x => x.Name, x => x);
            }
        }


    }
}
