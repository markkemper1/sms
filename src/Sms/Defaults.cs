using System.Collections.Generic;
using Sms.Messaging;
using Sms.Services;

namespace Sms
{
    public static class Defaults
    {
		public static ISerializerFactory SerializerFactory = Sms.Services.SerializerFactory.CreateEmpty();
        public static IServiceDefinitionRegistry ServiceDefinitionRegistry = new ServiceDefinitionRegistry();
	    public static List<IMessagingFactory> MessagingFactories = new List<IMessagingFactory>();
    }
}