using Sms.Messaging;

namespace Sms.Services
{
    public interface IMessageSink
    {
        void Send<T>(T item) where T : class, new();

        void Send(SmsMessage message);
    }
}