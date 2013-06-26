using System;

namespace Sms.Messaging
{
    public interface IReceiver<T> : IDisposable
    {
        string QueueName { get; }

        /// <summary>
        ///     Blocks until a single message is returned
        /// </summary>
        /// <returns></returns>
        Message<T> Receive(TimeSpan? timeout = null);
    }
}