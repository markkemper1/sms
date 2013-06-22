using System;

namespace Sms
{
    public class Message<T>
    {
        private readonly T message;
        private readonly Action<bool> processed;

        public Message(T message, Action<bool> processed) 
        {
            this.message = message;
            this.processed = processed;
        }
        public T Item {get { return message; }}

        public void Processed(bool successful)
        {
            processed(successful);
        }

        public void Success()
        {
            processed(true);
        }

        public void Failed()
        {
            processed(false);
        }
    }
}