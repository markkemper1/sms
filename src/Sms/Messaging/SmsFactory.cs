using System;
using System.Collections.Generic;
using System.Linq;

namespace Sms.Messaging
{
    public class SmsFactory
    {
 

        //private readonly Dictionary<Type, ServiceEndpointAttribute> typeUris = new Dictionary<Type, ServiceEndpointAttribute>();
        private Dictionary<string, IMessagingFactory> factories;

        static readonly object LockMe = new object();

        private static readonly SmsFactory instance = new SmsFactory();
        private static SmsFactory Instance { get { return instance; } }

        public static IMessageSender Sender(string provider, string queueName)
        {
            return Instance.GetFactory(provider).Sender(queueName);
        }

        public static IReciever<SmsMessage> Receiver(string provider, string url)
        {
            return Instance.GetFactory(provider).Reciever(url);
        }

      
        //private ServiceEndpointAttribute GetEndPoint(Type type)
        //{
        //    if (typeUris.ContainsKey(type))
        //        return typeUris[type];

        //    lock (LockMe)
        //    {
        //        if (typeUris.ContainsKey(type))
        //            return typeUris[type];

        //        var serviceEndpoint = (ServiceEndpointAttribute)Attribute.GetCustomAttribute(type, typeof(ServiceEndpointAttribute)) 
        //            ?? GetEndPointDefault(type);

        //        typeUris[type] = serviceEndpoint;

        //        return typeUris[type];
        //    }
        //}

        //private ServiceEndpointAttribute GetEndPointDefault(Type type)
        //{
        //    return new ServiceEndpointAttribute("broker", type.Name);
        //}

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
                factories = GenericFactory.FindAndBuild<IMessagingFactory>().ToDictionary(x=>x.Name, x=>x);
            }
        }

       
    }
}
