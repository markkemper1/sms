using System;
using System.Collections.Generic;
using Sms.Messaging;
using Sms.Routing;

namespace Sms.Services
{
    public class TypedMessageReceiver : IReceiver<object>
    {
        private IReceiver<SmsMessage> receiver;
        private readonly ServiceDefinitionRegistry registry;
        private Dictionary<string, Type> serviceTypes = new Dictionary<string, Type>();
        private Dictionary<string, ISerializer> serviceSerializers = new Dictionary<string, ISerializer>();
        private readonly SerializerFactory serializerFactory;

        public TypedMessageReceiver(IReceiver<SmsMessage> receiver, ServiceDefinitionRegistry registry, SerializerFactory serializerFactory)
        {
            this.receiver = receiver;
            this.registry = registry;
            this.serializerFactory = serializerFactory;
        }

        public void Configure<T>()
        {
            Configure(typeof(T));
        }

        public void Configure(Type type)
        {
            var serviceDefinition = registry.Get(type);
            serviceTypes[serviceDefinition.ServiceName] = type;
            var serializer = serializerFactory.Get(serviceDefinition.Serializer);
            serviceSerializers[serviceDefinition.ServiceName] = serializer;
        }

        public Message<object> Receive(TimeSpan? timeout = null)
        {
            var smsResult = receiver.Receive(timeout);

            if (smsResult == null)
                return null;

            if(!smsResult.Item.Headers.ContainsKey(RouterSettings.ServiceNameHeaderKey))
                throw new InvalidOperationException("Cannot receive messages without a service name header key");

            var serviceName = smsResult.Item.Headers[RouterSettings.ServiceNameHeaderKey];

            if (!serviceTypes.ContainsKey(serviceName))
                throw new InvalidOperationException(String.Format("Cannot receive the service item: {0} without configuration info that that service.", serviceName));

            var type = serviceTypes[serviceName];
            var serializer = serviceSerializers[serviceName];

            var item = serializer.Deserialize(type, smsResult.Item.Body);

            return new Message<object>(item, smsResult.Processed);
        }

        public void Dispose()
        {
            receiver.Dispose();
        }
    }
}
