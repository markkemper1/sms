using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sms.Messaging
{
    public class RecieveTask<T> : IDisposable
    {
        private Action<Message<T>> Action { get; set; }
        private readonly IReciever<T> reciever;
        private bool stopping;
        private Task task;

        public RecieveTask(IReciever<T> reciever, Action<Message<T>> action)
        {
            Action = action;
            this.reciever = reciever;
        }

        private bool Receiving { get; set; }

        public void Dispose()
        {
            this.Stop();
            reciever.Dispose();
        }

        public void Start()
        {
            if (task != null)
                return;

            Receiving = true;
            stopping = false;

            task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (!stopping)
                        {
                            var message = reciever.Receive(TimeSpan.FromMilliseconds(500));

                            if (stopping)
                                break;

                            if (message != null)
                            {
                                try
                                {
                                    Action(message);
                                }
                                catch
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
                });
        }

        public Exception Stop()
        {
            stopping = true;
            try
            {
                Task.WaitAll(task);
                return null;
            }
            catch (AggregateException ae)
            {
                ae.Flatten();
                return ae.InnerExceptions.First();
            }
        }
    }
}
