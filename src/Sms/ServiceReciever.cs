using System;
using Sms.Services;

namespace Sms
{
    public abstract class ServiceReciever<T> : IServiceReciever
    {
        public abstract void Process(Message<T> message);

        public Type MessageItemType { get { return typeof (T); } }

        public IMessageSink Sink { get; set; }

        public void Process(Message<object> message)
        {
            Process(new Message<T>((T)message.Item, message.Processed));
        }

        public static ServiceReciever<T> Create(Action<Message<T>> hander)
        {
            return new GenericServiceReciever(hander);
        }

        internal class GenericServiceReciever : ServiceReciever<T>
        {
            private readonly Action<Message<T>> hander;

            public GenericServiceReciever(Action<Message<T>> hander)
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
