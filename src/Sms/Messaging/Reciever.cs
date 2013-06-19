using System;

namespace Sms.Messaging
{
    public class Reciever : IMessageReciever
    {
        private readonly IMessageReciever reciever;
        private bool stopping;

        public Reciever(IMessageReciever reciever)
        {
            this.reciever = reciever;
        }

        public bool Receiving { get; private set; }


        public void Dispose()
        {
            this.Stop();
            reciever.Dispose();
        }


        public ReceivedMessage Receive(TimeSpan? timeout = null)
        {
            return reciever.Receive(timeout);
        }

        public void Subscribe(Action<ReceivedMessage> action)
        {
            Receiving = true;
            stopping = false;

            try
            {
                while (!stopping)
                {
                    var message = this.Receive(TimeSpan.FromMilliseconds(500));

                    if (stopping)
                        break;

                    if (message != null)
                    {
                        try
                        {
                            action(message);
                        }
                        catch (Exception ex)
                        {
                            message.Failed();
                            throw;
                        }
                    }
                }
            }
            finally
            {
                Receiving = false;
            }
        }

        public void Stop()
        {
            stopping = true;
        }
    }
}
