using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sms.Messaging;

namespace Sms.Services
{
    public class ServiceReceiver<T> : IReciever<T>
    {
        private readonly ISerializer serializer;
        private IMessageReciever reciever;

        public ServiceReceiver(IMessageReciever reciever, ISerializer serializer)
        {
            this.reciever = reciever;
            this.serializer = serializer;
        }

        public Result<T> Receive(TimeSpan? timeout = null)
        {
            var smsResult = reciever.Receive(timeout);

            if (smsResult == null)
                return null;

            var item = (T)serializer.Deserialize(typeof(T), smsResult.Item.Body);

            return new Result<T>(item, smsResult.Processed);
        }

        public void Dispose()
        {
            reciever.Dispose();
        }
    }
}
