using System;

namespace Sms.Messaging
{
    public interface IMessageSender<T> : IDisposable
    {
        void Send(T smsMessage);
    }
}