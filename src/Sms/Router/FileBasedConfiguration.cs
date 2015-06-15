using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace Sms.Router
{
	public class FileBasedConfiguration : IRouterConfiguration
	{
		private Dictionary<string, ServiceEndpoint> services = new Dictionary<string, ServiceEndpoint>();
		private Dictionary<string, List<string>> serviceMapping = new Dictionary<string, List<string>>();

		public IEnumerable<ServiceEndpoint> Get(string serviceName)
		{
			lock (this)
			{
				if (services == null)
					throw new InvalidOperationException("Service configuration not set, service name: " + serviceName);

				List<string> mappings;
				if (serviceMapping.TryGetValue(serviceName, out mappings))
				{
					if (mappings.All(x => services.ContainsKey(x)))
					{
						foreach (var item in mappings)
						{
							yield return services[item];
						}
					}
					else
					{
						yield break;
					}
				}

				if (!services.ContainsKey(serviceName))
					yield break;

				yield return services[serviceName];
			}
		}

		public void Add(ServiceEndpoint service)
		{
			lock (this)
			{
					services[service.MessageType] = service;
			}
		}

		public void AddMapping(string fromMessageType, string toMessageType, string version)
		{
			lock (this)
			{
				if (!serviceMapping.ContainsKey(fromMessageType))
					serviceMapping[fromMessageType] = new List<string>();

				serviceMapping[fromMessageType].RemoveAll(x => x == toMessageType);
				serviceMapping[fromMessageType].Add(toMessageType);
			}
		}

		public void Remove(ServiceEndpoint service)
		{
			lock (this)
			{
				if (!services.ContainsKey(service.MessageType))
				{
					return;
				}

				services.Remove(service.MessageType);
			}
		}

		public void RemoveMapping(string fromMessageType, string toMessageType)
		{
			lock (this)
			{
				if (!serviceMapping.ContainsKey(fromMessageType))
					return;

				serviceMapping[fromMessageType].RemoveAll(x => x == toMessageType);
			}
		}

		public void Clear(string queueIdentifier, string exceptVersion)
		{
			var servicesToRemove = services.Where(x => x.Value.QueueIdentifier == queueIdentifier && x.Value.Version != exceptVersion).Select(x=>x.Value).ToArray();

			services = services.Where(x => !servicesToRemove.Contains(x.Value)).ToDictionary(x => x.Key, x => x.Value);

			foreach (var map in serviceMapping.Values)
			{
				map.RemoveAll(target => servicesToRemove.Select(x => x.QueueIdentifier).Any(x => x == target));
			}
			
		}

		public void Load(IEnumerable<ServiceEndpoint> endpoints)
		{
			lock (this)
			{
				services = endpoints.ToDictionary(x => x.MessageType, x => x, StringComparer.InvariantCultureIgnoreCase);
			}
		}

		public static FileBasedConfiguration LoadConfiguration()
		{
			var section = (NameValueCollection)ConfigurationManager.GetSection("ServiceConfiguration");

			if (section == null)
			{
				Logger.Warn("Configuration could not be read");
				return new FileBasedConfiguration();
			}

			if (section.Keys.Count == 0)
			{
				Logger.Warn("Configuration does not contain any services, configuration key count == 0");
				return new FileBasedConfiguration();
			}

			var services = new List<ServiceEndpoint>();

			foreach (string key in section.Keys)
			{
				string value = section[key];

				var valueSplit = value.Split(new[] { "://" }, 2, StringSplitOptions.RemoveEmptyEntries);

				string provider = valueSplit[0];
				string queueName = valueSplit[1];

				services.Add(new ServiceEndpoint()
				{
					MessageType = key,
					ProviderName = provider,
					QueueIdentifier = queueName
				});
			}
			var result = new FileBasedConfiguration();
			result.Load(services);
			return result;
		}
	}
}