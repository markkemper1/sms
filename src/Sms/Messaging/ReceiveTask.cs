using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sms.Messaging
{
    public class ReceiveTask : IDisposable
    {
        private Action<MessageResult> Action { get; set; }
        private readonly IReceiver receiver;
        private bool stopping;
        private Task task;
        private readonly TimeSpan? receiveTimeOut;

        public ReceiveTask(IReceiver receiver, Action<MessageResult> action, TimeSpan? receiveTimeOut = null)
        {
            Action = action;
            this.receiver = receiver;
            this.receiveTimeOut = receiveTimeOut;
        }

        private bool Receiving { get; set; }

        public TaskStatus Status    
        {
            get { return task.Status; }
        }

        public void Dispose()
        {
            this.Stop();
            receiver.Dispose();
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
                        int noMessageCount = 0;
                        while (!stopping)
                        {
                            var message = receiver.Receive(receiveTimeOut ?? TimeSpan.FromMilliseconds(500));

                            if (stopping)
                                break;

                            if (message != null)
                            {
                                noMessageCount = 0;
                                try
                                {
                                    Action(message);
                                }
                                catch
                                {
                                    message.Failed();
                                }
                            }
                            else
                            {
                                noMessageCount ++;
                                Thread.Sleep( Math.Min(100 * noMessageCount,1000));
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
                if(task != null)
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
