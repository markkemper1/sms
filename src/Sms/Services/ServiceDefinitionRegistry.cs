using System;
using System.Collections.Generic;
using System.Linq;

namespace Sms.Services
{
    public class ServiceDefinitionRegistry : IServiceDefinitionRegistry
    {
        private Dictionary<Type, ServiceDefinition> typeConfigurations = new Dictionary<Type, ServiceDefinition>();

        public ServiceDefinition Get<T>()
        {
            return GetServiceConfiguration(typeof(T));
        }

        public ServiceDefinition Get(Type type)
        {
            return GetServiceConfiguration(type);
        }

        private ServiceDefinition GetServiceConfiguration(Type type)
        {
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

            var customHeaders = Attribute.GetCustomAttributes(type, typeof (ServiceHeaderAttribute)).Cast<ServiceHeaderAttribute>().ToArray();

            return new ServiceDefinition(attribute.RequestType)
            {
                Serializer = attribute.Serializer,
                Headers = customHeaders,
            };
        }

        private static ServiceDefinitionAttribute DefaultServiceDefinition(Type type)
        {
            return new ServiceDefinitionAttribute(type.Name, "json");
        }


    }

    public interface IServiceDefinitionRegistry
    {
        ServiceDefinition Get<T>();
        ServiceDefinition Get(Type type);
    }
}
