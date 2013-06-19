using System;
using System.Collections.Generic;
using System.Linq;

namespace Sms.RoutingService
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

        public void Load(IEnumerable<ServiceEndpoint> endpoints)
        {
            lock (LockMe)
            {
                services = endpoints.ToDictionary(x => x.ServiceName, x => x, StringComparer.InvariantCultureIgnoreCase);
            }
        }
    }
}