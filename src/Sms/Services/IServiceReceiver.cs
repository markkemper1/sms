using System;

namespace Sms.Services
{
    public interface IServiceReceiver
    {
        Type MessageItemType { get; }

        IMessageSink Sink { get; set; }

        void Process(Message<object> message);
    }
}