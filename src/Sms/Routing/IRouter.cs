using System;
using Sms.Messaging;

namespace Sms.Routing
{
    public interface IRouter : IDisposable
    {
        void Send(string serviceName, string message);
        IReciever<SmsMessage> Receiver(string serviceName);
        
    }
}