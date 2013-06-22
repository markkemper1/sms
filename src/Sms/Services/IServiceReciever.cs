using System;

namespace Sms.Services
{
    public interface IServiceReciever
    {
        Type MessageItemType { get; }

        IMessageSink Sink { get; set; }

        void Process(Message<object> message);
    }
}