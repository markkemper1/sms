using System;

namespace Sms.Services
{
    public class ServiceDefinition
    {
        public ServiceDefinition(string requestTypeName)
        {
            RequestTypeName = requestTypeName;
        }

        public string RequestTypeName { get; set; }

        public string Serializer { get; set; }

        public ServiceHeaderAttribute[] Headers { get; set; }
    }

    public class ServiceHeaderAttribute : Attribute
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public ServiceHeaderAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}