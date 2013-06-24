using System;
using System.Collections.Generic;
using Sms.Messaging;

namespace Sms.Routing
{
    public class BrokerProxingReceiver : IReceiver<SmsMessage>
    {
        private IMessageSender<SmsMessage> SendNextMessage { get; set; }
        private IReceiver<SmsMessage> Receiver { get; set; }
        private string ServiceName { get; set; }
        private bool outstanding = false;

        public BrokerProxingReceiver(IMessageSender<SmsMessage> sendNextMessage, IReceiver<SmsMessage> receiver, string serviceName)
        {
            if (sendNextMessage == null) throw new ArgumentNullException("sendNextMessage");
            if (receiver == null) throw new ArgumentNullException("receiver");
            SendNextMessage = sendNextMessage;
            Receiver = receiver;
            ServiceName = serviceName;
        }

        public Message<SmsMessage> Receive(TimeSpan? timeout = null)
        {
            if (!outstanding)
            {
                SendNextMessage.Send(new SmsMessage(ServiceName, null, new Dictionary<string, string>()
                    {
                        {RouterSettings.ServiceNameHeaderKey, ServiceName}
                    }));
            }

            var receivedMessage = Receiver.Receive(timeout);

            outstanding = receivedMessage == null;

            return receivedMessage;
        }

        public void Dispose()
        {
            Receiver.Dispose();
        }
    }
}