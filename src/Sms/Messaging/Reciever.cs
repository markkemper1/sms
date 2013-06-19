using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sms.Messaging
{
    //public class Reciever : Reciever<SmsMessage>
    //{
    //    public Reciever(IReciever<SmsMessage> reciever)
    //        : base(reciever)
    //    {

    //    }
    //}

    //public class Reciever<T> : IReciever<T>
    //{
    //    private readonly IReciever<T> reciever;
    //    private bool stopping;

    //    public Reciever(IReciever<T> reciever)
    //    {
    //        this.reciever = reciever;
    //    }

    //    public bool Receiving { get; private set; }

    //    public void Dispose()
    //    {
    //        this.Stop();
    //        reciever.Dispose();
    //    }

    //    public Result<T> Receive(TimeSpan? timeout = null)
    //    {
    //        return reciever.Receive(timeout);
    //    }

    //    public void Subscribe(Action<Result<T>> action)
    //    {
    //        Receiving = true;
    //        stopping = false;

    //        try
    //        {
    //            while (!stopping)
    //            {
    //                var message = this.Receive(TimeSpan.FromMilliseconds(500));

    //                if (stopping)
    //                    break;

    //                if (message != null)
    //                {
    //                    try
    //                    {
    //                        action(message);
    //                    }
    //                    catch 
    //                    {
    //                        message.Failed();
    //                        throw;
    //                    }
    //                }
    //            }
    //        }
    //        finally
    //        {
    //            Receiving = false;
    //        }
    //    }

    //    public void Stop()
    //    {
    //        stopping = true;
    //    }
    //}

    public class RecieveTask<T> : IDisposable
    {
        private Action<Result<T>> Action { get; set; }
        private readonly IReciever<T> reciever;
        private bool stopping;
        private Task task;

        public RecieveTask(IReciever<T> reciever, Action<Result<T>> action)
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
