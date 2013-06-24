using System;
using Sms.Services;

namespace Sms
{
    public abstract class ServiceReceiver<T> : IServiceReceiver
    {
        public abstract void Process(Message<T> message);

        public Type MessageItemType { get { return typeof (T); } }

        public IMessageSink Sink { get; set; }

        public void Process(Message<object> message)
        {
            Process(new Message<T>((T)message.Item, message.Processed));
        }

        public static ServiceReceiver<T> Create(Action<Message<T>> hander)
        {
            return new GenericServiceReceiver(hander);
        }

        internal class GenericServiceReceiver : ServiceReceiver<T>
        {
            private readonly Action<Message<T>> hander;

            public GenericServiceReceiver(Action<Message<T>> hander)
            {
                this.hander = hander;
            }

            public override void Process(Message<T> message)
            {
                hander(message);
            }
        }
    }
}
