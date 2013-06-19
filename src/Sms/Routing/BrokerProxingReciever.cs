using System;
using System.Collections.Generic;
using Sms.Messaging;

namespace Sms.Routing
{
    public class BrokerProxingReciever : IReciever<SmsMessage>
    {
        private IMessageSender SendNextMessage { get; set; }
        private IReciever<SmsMessage> Reciever { get; set; }
        private string ServiceName { get; set; }
        private bool outstanding = false;

        public BrokerProxingReciever(IMessageSender sendNextMessage, IReciever<SmsMessage> reciever, string serviceName)
        {
            if (sendNextMessage == null) throw new ArgumentNullException("sendNextMessage");
            if (reciever == null) throw new ArgumentNullException("reciever");
            SendNextMessage = sendNextMessage;
            Reciever = reciever;
            ServiceName = serviceName;
        }

        public Result<SmsMessage> Receive(TimeSpan? timeout = null)
        {
            if (!outstanding)
            {
                SendNextMessage.Send(new SmsMessage(ServiceName, null, new Dictionary<string, string>()
                    {
                        {RouterSettings.ServiceNameHeaderKey, ServiceName}
                    }));
            }

            var receivedMessage = Reciever.Receive(timeout);

            outstanding = receivedMessage == null;

            return receivedMessage;
        }

        public void Dispose()
        {
            Reciever.Dispose();
        }
    }
}