using System;

namespace Sms.Services
{
    public class ServiceDefinitionAttribute: Attribute
    {
        public string ServiceName { get; set; }

        public string Serializer { get; set; }

        public ServiceDefinitionAttribute(string serviceName, string serializer)
        {
            ServiceName = serviceName;
            Serializer = serializer;
        }
    }
}
