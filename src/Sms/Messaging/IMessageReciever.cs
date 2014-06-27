using System;

namespace Sms.Messaging
{
    public interface IReceiver : IDisposable
    {
        string ProviderName { get;  }

        string QueueName { get; }

        /// <summary>
        ///     Blocks until a single message is returned
        /// </summary>
        /// <returns></returns>
        MessageResult Receive(TimeSpan? timeout = null);
    }
}