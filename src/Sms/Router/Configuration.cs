using System;
using System.Collections.Generic;
using System.Linq;

namespace Sms.Router
{
    public class Configuration
    {
        private readonly static object LockMe = new object();

        private Dictionary<string, ServiceEndpoint> services;

        public ServiceEndpoint Get(string serviceName)
        {
            if(services == null)
                throw new InvalidOperationException("Service configuration not set, service name: " + serviceName);

            if(!services.ContainsKey(serviceName))
                throw new ArgumentException("Service does not exist in configuration. Service name: " + serviceName );

            return services[serviceName];
        }

        public bool IsKnown(string serviceName)
        {
            if (services == null)
                return false;

            if (!services.ContainsKey(serviceName))
                return false;

            return true;
        }

        public void Load(IEnumerable<ServiceEndpoint> endpoints)
        {
            lock (LockMe)
            {
                services = endpoints.ToDictionary(x => x.MessageType, x => x, StringComparer.InvariantCultureIgnoreCase);
            }
        }
    }
}