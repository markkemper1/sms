using System;

namespace Sms.Messaging
{
    public interface IReciever<T> : IDisposable
    {
        /// <summary>
        ///     Blocks until a single message is returned
        /// </summary>
        /// <returns></returns>
        Result<T> Receive(TimeSpan? timeout = null);

        ///// <summary>
        /////     Blocks and calls action on every message
        ///// </summary>
        ///// <param name="action"></param>
        //void Subscribe(Func<SmsMessage, bool> action);
    }
}