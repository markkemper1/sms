using System;
using System.Collections.Generic;
using Sms.Messaging;

namespace Sms.Services
{
    public class ServiceReceiverTask : IDisposable
    {
        private TypedMessageReceiver receiver;
        private ReceiveTask<object> task;
        GenericMessageHandler handler = new GenericMessageHandler();

        public ServiceReceiverTask(TypedMessageReceiver receiver)
        {
            this.receiver = receiver;
            this.task = new ReceiveTask<object>(receiver, HandleMessage);
        }

        public void Register(Type serviceType, Action<Message<object>> action)
        {
            receiver.Configure(serviceType);
            handler.Register(serviceType, action);
        }

        private void HandleMessage(Message<object> message)
        {
            handler.Process(message);
        }

        public void Start()
        {
            task.Start();
        }

        public Exception Stop()
        {
            return task.Stop();
        }

        public void Dispose()
        {
            task.Dispose();
        }
    }
}
