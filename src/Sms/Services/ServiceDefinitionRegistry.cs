using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var customHeaders = Attribute.GetCustomAttributes(type, typeof(ServiceHeaderAttribute)).Cast<ServiceHeaderAttribute>().ToArray();

            return new ServiceDefinition(attribute.RequestType)
            {
                Serializer = attribute.Serializer,
                Headers = customHeaders,
            };
        }

        private static ServiceDefinitionAttribute DefaultServiceDefinition(Type type)
        {
            var generateTypeName = GenerateTypeName(type);
            return new ServiceDefinitionAttribute(generateTypeName, "json");
        }

        public static string GenerateTypeName(Type type)
        {
            var sb = new StringBuilder();
            GenerateTypeName(type, sb);
            return sb.ToString();
        }
        private static void GenerateTypeName(Type type, StringBuilder sb)
        {
            var indexOfGenericMark = type.Name.IndexOf('`');
            sb.Append(indexOfGenericMark < 0 ? type.Name : type.Name.Substring(0, indexOfGenericMark));

            if (indexOfGenericMark >= 0)
            {
                var generics = type.GetGenericArguments();
                if (generics.Length > 0)
                {
                    sb.Append("<");
                    foreach (var g in generics)
                    {
                        GenerateTypeName(g, sb);
                        sb.Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(">");
                }
            }
        }
    }

    public interface IServiceDefinitionRegistry
    {
        ServiceDefinition Get<T>();
        ServiceDefinition Get(Type type);
    }
}
