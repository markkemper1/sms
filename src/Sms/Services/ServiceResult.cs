using System;

namespace Sms.Services
{
    public class ServiceMessage<T> : IDisposable
    {
        private readonly MessageResult message;
        private bool processed = false;

        public ServiceMessage(T item, MessageResult message)
        {
            Item = item;
            this.message = message;
        }

        public T Item { get; private set; }

        public void Processed(bool successful = true)
        {
            message.Processed(successful);
        }

        public void Success()
        {
            message.Processed(true);
        }

        public void Failed()
        {
            message.Processed(false);
        }

        public void Dispose()
        {
            if(!processed)
                message.Processed(false);
        }
    }
}