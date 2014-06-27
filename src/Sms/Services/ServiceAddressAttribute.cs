using System;

namespace Sms.Services
{
    public class ServiceDefinitionAttribute: Attribute
    {
        public string Serializer { get; set; }

        public string RequestType { get; set; }

        public ServiceDefinitionAttribute(string requestType, string serializer)
        {
            RequestType = requestType;
            Serializer = serializer;
        }
    }
}
