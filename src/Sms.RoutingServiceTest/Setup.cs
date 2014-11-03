using DotNetConfigHelper;
using NUnit.Framework;
using Sms.Msmq;

namespace Sms.RoutingServiceTest
{
	[SetUpFixture]
    public class Setup
    {
		[SetUp]
		public void register()
		{
			AppSettingsReplacer.Install(DotNetConfigHelper.ConfigProvider.CreateAndSetDefault());
			Defaults.MessagingFactories.Add(new MsmqFactory());
		}
    }
}
