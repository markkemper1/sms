using System.Collections.Generic;

namespace Sms.Router
{
	public interface IRouterConfiguration
	{
		IEnumerable<ServiceEndpoint> Get(string serviceName);

		void Add(ServiceEndpoint service);
		void AddMapping(string fromMessageType, string toMessageType);
		void Remove(ServiceEndpoint service);
		void RemoveMapping(string fromMessageType, string toMessageType);


	}
}