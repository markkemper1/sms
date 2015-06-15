using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using NUnit.Framework;
using Sms.Router;

namespace Sms.Test.Routing
{
	[TestFixture]
	public class RdsConfigurationTest
	{
		private RdsBasedConfiguration rdsConfig;

		[SetUp]
		public void SetUp()
		{
			var config = ConfigurationManager.ConnectionStrings[0];

			Func<IDbConnection> getConnection = () =>
			{
				DbProviderFactory dbProvider = DbProviderFactories.GetFactory(config.ProviderName);
				var connection = dbProvider.CreateConnection();
				connection.ConnectionString = config.ConnectionString;
				return connection;
			};
			rdsConfig = new RdsBasedConfiguration(getConnection, "TestEndPoint", "TestRouting");
			rdsConfig.EnsureTableExists();
		}


		[Test]
		public void should_add_and_remove_item()
		{
			var service = new ServiceEndpoint()
			{
				MessageType = "Hello",
				ProviderName = "msmq",
				QueueIdentifier = "testing"
			};

			rdsConfig.Add(service);

			var results = rdsConfig.Get("Hello");

			Assert.That(results, Is.Not.Null);

			var service2 = new ServiceEndpoint()
			{
				MessageType = "Hello",
				ProviderName = "msmq",
				QueueIdentifier = "testing2"
			};

			rdsConfig.Add(service2);

			results = rdsConfig.Get("Hello");

			Assert.That(results.Count(), Is.EqualTo(1));

			rdsConfig.Remove(service2);

			results = rdsConfig.Get("Hello");

			Assert.That(results.Count(), Is.EqualTo(0));
		}

		[Test]
		public void should_be_able_to_do_1_to_many_mapping()
		{
			var service = new ServiceEndpoint()
			{
				MessageType = "Hello1",
				ProviderName = "msmq",
				QueueIdentifier = "testing"
			};

			var service2 = new ServiceEndpoint()
			{
				MessageType = "Hello2",
				ProviderName = "msmq",
				QueueIdentifier = "testing2"
			};


			rdsConfig.Add(service);
			rdsConfig.Add(service2);

			rdsConfig.AddMapping("HelloRoot", "Hello1", "1");
			rdsConfig.AddMapping("HelloRoot", "Hello2", "1");

			var results = rdsConfig.Get("HelloRoot");

			Assert.That(results, Is.Not.Null);


			results = rdsConfig.Get("HelloRoot");

			Assert.That(results.Count(), Is.EqualTo(2));


			rdsConfig.RemoveMapping("HelloRoot", "Hello1");

			results = rdsConfig.Get("HelloRoot");

			Assert.That(results.Count(), Is.EqualTo(1));

			rdsConfig.RemoveMapping("HelloRoot", "Hello2");

			results = rdsConfig.Get("HelloRoot");

			Assert.That(results.Count(), Is.EqualTo(0));

			rdsConfig.Remove(service2);
		}


		[Test]
		public void should_clear_all_items_without_version()
		{
			var service = new ServiceEndpoint()
			{
				MessageType = "Hello1",
				ProviderName = "msmq",
				QueueIdentifier = "testing",
				Version = "1.0"
			};

			var service2 = new ServiceEndpoint()
			{
				MessageType = "Hello2",
				ProviderName = "msmq",
				QueueIdentifier = "testing2",
				Version = "1.0"
			};

			var service3 = new ServiceEndpoint()
			{
				MessageType = "Hello1",
				ProviderName = "msmq",
				QueueIdentifier = "testing",
				Version = "2.0"
			};

			var service4 = new ServiceEndpoint()
			{
				MessageType = "Hello2",
				ProviderName = "msmq",
				QueueIdentifier = "testing2",
				Version = "2.0"
			};


			rdsConfig.Add(service);
			rdsConfig.Add(service2);

			rdsConfig.AddMapping("HelloRoot", "Hello1", null);
			rdsConfig.AddMapping("HelloRoot", "Hello2", "2");

			rdsConfig.Clear(service.QueueIdentifier, "1.0");
			rdsConfig.Clear(service2.QueueIdentifier, "1.0");

			Assert.That(rdsConfig.Get("HelloRoot").Count(), Is.EqualTo(1));
			Assert.That(rdsConfig.Get("Hello1").Count(), Is.EqualTo(1));
			Assert.That(rdsConfig.Get("Hello2").Count(), Is.EqualTo(1));
		}
	}

}
