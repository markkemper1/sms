using System;

namespace Sms.Messaging
{
    public interface IMessageSender : IDisposable
    {
        void Send(SmsMessage smsMessage);
    }
}