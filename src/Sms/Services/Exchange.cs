using System;
using System.Collections.Generic;
using Sms.Messaging;
using Sms.Routing;

namespace Sms.Services
{
    public class Exchange : IDisposable
    {
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

            var serializer = GetSeralizer(config.Serializer);

            Router.Send(config.ServiceName, serializer.Serialize(request));
        }

        public Result<T> ReceiveOne<T>(TimeSpan? timeout = null) where T : class, new()
        {
            var config = GetServiceConfiguration<T>();

            using (var receiver = CreateReciever<T>())
            {
                var result = receiver.Receive(timeout);
                return result;
            }
        }

        public RecieveTask<T> Receiver<T>(Action<Result<T>> action) where T : class, new()
        {
            return new RecieveTask<T>(this.CreateReciever<T>(), action);
        }

        private IReciever<T> CreateReciever<T>()
        {
            var config = GetServiceConfiguration<T>();
            var receiver = Router.Receiver(config.ServiceName);
             var serializer = GetSeralizer(config.Serializer);
             return new ServiceReceiver<T>(receiver, serializer);
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
            if(Router != null)
                Router.Dispose();
        }
    }
}
