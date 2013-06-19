using System;
using System.Collections.Generic;
using Sms.Messaging;
using Sms.Routing;

namespace Sms.Services
{
    public class Exchange : IDisposable
    {
        IDictionary<string, IMessageReciever> receivers = new Dictionary<string, IMessageReciever>();
        private ReceivedMessage CurrentMessage;

        private IRouter Router { get; set; }

        public Exchange()
            : this(RouterFactory.Build())
        {
        }

        public Exchange(IRouter router)
        {
            Router = router;
        }

        public void Send<T>(T request) where T : class, new()
        {
            var config = GetServiceConfiguration<T>();

            ISerializer serializer = GetSeralizer(config.Serializer);

            Router.Send(config.ServiceName, serializer.Serialize(request));
        }

        public T Receive<T>(TimeSpan? timeout = null) where T : class, new()
        {
            if (CurrentMessage != null)
                throw new ArgumentException("You must call message processed before receiving more messages");

            var config = GetServiceConfiguration<T>();

            var receiver = GetReciever(config.ServiceName);

            CurrentMessage = receiver.Receive(timeout);

            if (CurrentMessage == null)
                return default(T);

            var serializer = GetSeralizer(config.Serializer);

            return (T)serializer.Deserialize(typeof(T), CurrentMessage.Body);
        }

        public void Processed(bool successfully = true)
        {
            if (successfully)
                CurrentMessage.Success();
            else
                CurrentMessage.Failed();

        }

        private IMessageReciever GetReciever(string serviceName)
        {
            if (!receivers.ContainsKey(serviceName))
            {
                lock (receivers)
                {
                    if (!receivers.ContainsKey(serviceName))
                    {
                        receivers[serviceName] = Router.Receiver(serviceName);
                    }
                }
            }

            return receivers[serviceName];
        }

        private Dictionary<string, ISerializer> serializers = new Dictionary<string, ISerializer>();

        private ISerializer GetSeralizer(string serializerName)
        {
            if (serializers.Count == 0)
            {
                lock (serializers)
                {
                    if (serializers.Count == 0)
                    {
                        foreach (var item in GenericFactory.FindAndBuild<ISerializer>())
                        {
                            serializers.Add(item.Name, item);
                        }
                    }
                }
            }

            if (!serializers.ContainsKey(serializerName))
            {
                throw new ArgumentException("Not serializer found for the name: " + serializerName);
            }

            return serializers[serializerName];
        }

        private Dictionary<Type, ServiceDefinition> typeConfigurations = new Dictionary<Type, ServiceDefinition>();

        private ServiceDefinition GetServiceConfiguration<T>()
        {
            var type = typeof(T);

            if (!typeConfigurations.ContainsKey(type))
            {
                lock (typeConfigurations)
                {
                    if (!typeConfigurations.ContainsKey(type))
                    {
                        typeConfigurations.Add(type, CreateServiceDefinition(type));
                    }
                }
            }
            return typeConfigurations[type];
        }

        private static ServiceDefinition CreateServiceDefinition(Type type)
        {
            var attribute = (ServiceDefinitionAttribute)Attribute.GetCustomAttribute(type, typeof(ServiceDefinitionAttribute)) ?? DefaultServiceDefinition(type);

            return new ServiceDefinition()
                {
                    Serializer = attribute.Serializer,
                    ServiceName = attribute.ServiceName
                };
        }

        private static ServiceDefinitionAttribute DefaultServiceDefinition(Type type)
        {
            return new ServiceDefinitionAttribute(type.Name, "json");
        }

        public void Dispose()
        {
            lock (receivers)
            {
                foreach (var r in receivers)
                {
                    r.Value.Dispose();
                }
                receivers.Clear();
            }

            Router.Dispose();
        }
    }
    // serialize
}
