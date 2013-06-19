using System;
using System.Collections.Generic;
using Sms.Messaging;

namespace Sms.Routing
{
    public class Router : IRouter
    {
        private static readonly object lockMe = new object();
        private IMessageSender SignalNextMessage { get; set; }
        private Func<string, IReciever<SmsMessage>> ReceiverFactory { get; set; }
        private readonly IMessageSender viaBrokerSender;

        public Router(IMessageSender viaBrokerSender, IMessageSender signalNextMessage, Func<string, IReciever<SmsMessage>> receiverFactory)
        {
            SignalNextMessage = signalNextMessage;
            ReceiverFactory = receiverFactory;
            this.viaBrokerSender = viaBrokerSender;
        }

        public void Send(string serviceName, string message)
        {
            viaBrokerSender.Send(new SmsMessage(serviceName, message, new Dictionary<string, string>
                {
                    {RouterSettings.ServiceNameHeaderKey,serviceName}
                }));
        }

        public IReciever<SmsMessage> Receiver(string serviceName)
        {
            var receiver = ReceiverFactory(serviceName);

            var proxy = new BrokerProxingReciever(SignalNextMessage, receiver, serviceName);

            return proxy;
        }

        private static Router instance;

        public static Router Instance
        {
            get
            {
                if (instance != null) return instance;

                lock (lockMe)
                {
                    if (instance != null) return instance;

                    instance = RouterFactory.Build();

                    return instance;
                }
            }
        }

        public void Dispose()
        {
            SignalNextMessage.Dispose();
            viaBrokerSender.Dispose();
        }
    }
}
