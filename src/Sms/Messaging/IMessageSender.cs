using System;

namespace Sms.Messaging
{
    public interface IMessageSink : IDisposable
    {
        string ProviderName { get; }

        string QueueName { get; }

        void Send(SmsMessage smsMessage);
    }
}