using System;
using System.Collections.Generic;

namespace Sms.Services
{
    public class GenericMessageHandler  
    {
        private readonly IDictionary<Type, Action<Message<object>>> handlers = new Dictionary<Type, Action<Message<object>>>();

        public void Register(Type serviceType, Action<Message<object>> action)
        {
            var type = serviceType;

            if (handlers.ContainsKey(type))
                throw new InvalidOperationException("This type has already got a handler registered");

            handlers[type] = action;
        }

     

        public void Process(Message<object> message)
        {
            if (message == null) throw new ArgumentNullException("message");

            var type = message.Item.GetType();

            if(!handlers.ContainsKey(type))
                throw new ArgumentException("There is no handler registered for the generic message type of: " + type. FullName);

            handlers[type](message);
        }
    }
}
