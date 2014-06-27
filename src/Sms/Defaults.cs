using Sms.Services;

namespace Sms
{
    public static class Defaults
    {
        public static ISerializerFactory SerializerFactory = new SerializerFactory();
        public static IServiceDefinitionRegistry ServiceDefinitionRegistry = new ServiceDefinitionRegistry();
    }
}