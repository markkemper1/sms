using System;
using System.Linq;

namespace Sms.Messaging
{
	public class SmsFactory
	{

		public static IMessageSink Sender(string provider, string queueName)
        {
			var factory = Defaults.MessagingFactories.FirstOrDefault(x => x.Name == provider);
			if (factory == null) throw new ArgumentException("The provider name \"" + provider + "\" has not been registered in the Defaults.MessagingFactories list", "provider");
	        return factory.Sender(queueName);
        }

		public static IReceiver Receiver(string provider, string url)
		{
			var factory = Defaults.MessagingFactories.FirstOrDefault(x => x.Name == provider);
			if (factory == null) throw new ArgumentException("The provider name \"" + provider + "\" has not been registered in the Defaults.MessagingFactories list", "provider");
	        return factory.Receiver(url);
		}


		//EVIL
		//private void CreateFactories()
		//{
		//	if (factories != null) return;

		//	lock (LockMe)
		//	{
		//		if (factories != null) return;
		//		factories = GenericFactory.FindAndBuild<IMessagingFactory>().ToDictionary(x => x.Name, x => x);
		//	}
		//}


	}
}
