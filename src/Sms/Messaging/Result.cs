using System;

namespace Sms.Messaging
{
    public class Result<T>
    {
        private readonly T message;
        private readonly Action<bool> processed;

        public Result(T message, Action<bool> processed) 
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