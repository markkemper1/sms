using System;
using System.Collections.Generic;

namespace Sms.Services
{
    public class ServiceDefinitionRegistry
    {
        private Dictionary<Type, ServiceDefinition> typeConfigurations = new Dictionary<Type, ServiceDefinition>();

        static readonly object LockMe = new object();

        private static readonly ServiceDefinitionRegistry Instance = new ServiceDefinitionRegistry();

        public ServiceDefinition Get<T>()
        {
            return GetServiceConfiguration(typeof(T));
        }

        public ServiceDefinition Get(Type type)
        {
            return GetServiceConfiguration(type);
        }

        //public static ServiceDefinition Get<T>()
        //{
        //    return Instance.GetServiceConfiguration(typeof(T));
        //}


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


    }
}
