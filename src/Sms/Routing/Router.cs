using System;
using System.Collections.Generic;
using Sms.Messaging;

namespace Sms.Routing
{
    public class Router : IRouter
    {
        private static readonly object lockMe = new object();
        private IMessageSender<SmsMessage> SignalNextMessage { get; set; }
        private Func<string, IReciever<SmsMessage>> ReceiverFactory { get; set; }
        private readonly IMessageSender<SmsMessage> viaBrokerSender;

        public Router(IMessageSender<SmsMessage> viaBrokerSender, IMessageSender<SmsMessage> signalNextMessage, Func<string, IReciever<SmsMessage>> receiverFactory)
        {
            SignalNextMessage = signalNextMessage;
            ReceiverFactory = receiverFactory;
            this.viaBrokerSender = viaBrokerSender;
        }

        public void Send(string serviceName, string message, IDictionary<string,string> headers = null )
        {
            headers = headers ?? new Dictionary<string, string>();
            headers[RouterSettings.ServiceNameHeaderKey] = serviceName;
            viaBrokerSender.Send(new SmsMessage(serviceName, message, headers));
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
