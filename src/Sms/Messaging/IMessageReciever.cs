using System;

namespace Sms.Messaging
{
    public interface IReciever<T> : IDisposable
    {
        /// <summary>
        ///     Blocks until a single message is returned
        /// </summary>
        /// <returns></returns>
        Message<T> Receive(TimeSpan? timeout = null);
    }
}