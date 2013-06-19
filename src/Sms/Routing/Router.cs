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
            string recieveQueue = Guid.NewGuid().ToString();
            var receiver = ReceiverFactory(recieveQueue);

            var proxy = new BrokerProxingReciever(SignalNextMessage, receiver, serviceName, recieveQueue);

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

    public class BrokerProxingReciever : IReciever<SmsMessage>
    {
        private IMessageSender SendNextMessage { get; set; }
        private IReciever<SmsMessage> Reciever { get; set; }
        private string ServiceName { get; set; }
        public string RecieveQueueName { get; private set; }

        public BrokerProxingReciever(IMessageSender sendNextMessage, IReciever<SmsMessage> reciever, string serviceName, string recieveQueueName)
        {
            if (sendNextMessage == null) throw new ArgumentNullException("sendNextMessage");
            if (reciever == null) throw new ArgumentNullException("reciever");
            SendNextMessage = sendNextMessage;
            Reciever = reciever;
            ServiceName = serviceName;
            RecieveQueueName = recieveQueueName;
        }

        public Result<SmsMessage> Receive(TimeSpan? timeout = null)
        {
            SendNextMessage.Send(new SmsMessage(ServiceName, RecieveQueueName, new Dictionary<string, string>()
                {
                    {RouterSettings.ServiceNameHeaderKey, ServiceName}
                }));

            var receivedMessage = Reciever.Receive(timeout);

            return receivedMessage;
        }

        public void Dispose()
        {
            Reciever.Dispose();
        }
    }
}
