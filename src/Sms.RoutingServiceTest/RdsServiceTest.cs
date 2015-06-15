using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Sms.Messaging;
using Sms.Msmq;
using Sms.Router;

namespace Sms.RoutingServiceTest
{
	[TestFixture]
	public class RdsServiceTest
	{
		private RdsBasedConfiguration rdsConfig;

		[SetUp]
		public void SetUp()
		{
			Defaults.MessagingFactories.Add(new MsmqFactory());
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
		public void should_send_messages()
		{
			int receivedCount = 0;

			var receive = new ReceiveTask(SmsFactory.Receiver("msmq", "helloWorldService"), message =>
				{
					receivedCount++;
					message.Success();
				});


			receive.Start();

			var router = new RouterService(rdsConfig);
			var serviceEndpoint = new ServiceEndpoint()
			{
				ProviderName = "msmq",
				MessageType = "testService",
				QueueIdentifier = "helloWorldService"
			};
			router.Config.Add(
                    serviceEndpoint
                );

			router.Start();

			Thread.Sleep(1000);

			var watch = new Stopwatch();

			//warm up

			RouterSink.Default.Send("testService", "Test me, hello?");

			watch.Start();
			for (int i = 0; i < 1000; i++)
			{
				RouterSink.Default.Send("testService", "Test me, hello?");
			}
			watch.Stop();
			Console.WriteLine("Send 1000 in : " + watch.ElapsedMilliseconds);

			Thread.Sleep(5000);

			router.Stop();

			receive.Dispose();

			Thread.Sleep(2000);

			Assert.That(receivedCount, Is.EqualTo(1001));
		}

		[Test]
		public void should_send_message_to_multiple_endpoints()
		{
			int receivedCount = 0;

			var receiver1 = new ReceiveTask(SmsFactory.Receiver("msmq", "helloWordService"), message =>
			{
				receivedCount++;
				message.Success();
			});


			receiver1.Start();

			var router = new RouterService(rdsConfig);
			

			router.Start();

			Thread.Sleep(1000);

			RouterSink.Default.ConfigureEndpoint("testService1", "msmq", "helloWordService", "1");
			RouterSink.Default.ConfigureEndpoint("testService2", "msmq", "helloWordService", "1");
			RouterSink.Default.ConfigureMapping("testService", "testService1", "1");
			RouterSink.Default.ConfigureMapping("testService", "testService2", "1");

			Thread.Sleep(2000);
			
			RouterSink.Default.Send("testService", "Test me, hello?");
			
			Thread.Sleep(2000);


			router.Stop();

			receiver1.Dispose();

			Thread.Sleep(2000);

			Assert.That(receivedCount, Is.EqualTo(2));
		}

		//[Test]
		//public void should_proxy_receive_messages()
		//{
		//	var sender = SmsFactory.Sender("msmq", "helloWorldService_Sending");

		//	int receivedCount = 0;

		//	sender.Send(new SmsMessage("helloWorldService_Sending", "hello there !"));


		//	var router = new RouterService();

		//	router.Config.Load(new List<ServiceEndpoint>()
		//		{
		//			new ServiceEndpoint()
		//				{
		//					ProviderName = "msmq",
		//					ServiceName = "testService_send",
		//					QueueIdentifier = "helloWorldService_Sending"
		//				}
		//		});

		//	router.Start();

		//	ReceiveTask<SmsMessage> receiver = null;
		//	Thread.Sleep(1000);


		//	receiver = new ReceiveTask<SmsMessage>(Router.Instance.Receiver("testService_send"), message =>
		//		 {
		//			 receivedCount++;
		//			 Console.WriteLine(message.Item.Body);
		//			 message.Success();
		//		 });

		//	receiver.Start();

		//	Thread.Sleep(1000);

		//	router.Stop();

		//	receiver.Dispose();

		//	Thread.Sleep(1000);

		//	Assert.That(receivedCount, Is.EqualTo(1));
		//}


		//[Test]
		//public void should_put_unknown_messages_on_error_queue()
		//{
		//	int receivedCount = 0;
		//	var receiver = new ReceiveTask<SmsMessage>(SmsFactory.Receiver("msmq", RouterSettings.SendErrorQueueName), message =>
		//	{
		//		receivedCount++;
		//		message.Success();
		//	});

		//	receiver.Start();

		//	var router = new RouterService();

		//	router.Start();

		//	Thread.Sleep(1000);

		//	Stopwatch watch = new Stopwatch();

		//	//warm up
		//	Router.Instance.Send("testService", "Test me, hello?");

		//	Thread.Sleep(1000);

		//	router.Stop();

		//	receiver.Dispose();

		//	Thread.Sleep(1000);

		//	Assert.That(receivedCount, Is.EqualTo(1));



		//}

		//[Test]
		//public void should_process_errors_on_startup()
		//{
		//	int receivedCount = 0;

		//	var receiveTask = new ReceiveTask<SmsMessage>(SmsFactory.Receiver("msmq", "helloWorldService"), message =>
		//	{
		//		receivedCount++;
		//		message.Success();
		//	});

		//	receiveTask.Start();

		//	var router = new RouterService();

		//	router.Start();

		//	Thread.Sleep(1000);

		//	//warm up
		//	Router.Instance.Send("testService", "Test me, hello?");

		//	Thread.Sleep(1000);

		//	router.Stop();

		//	Thread.Sleep(1000);

		//	Assert.That(receivedCount, Is.EqualTo(0));


		//	router = new RouterService();
		//	router.Config.Load(new List<ServiceEndpoint>()
		//		{
		//			new ServiceEndpoint()
		//				{
		//					ProviderName = "msmq",
		//					ServiceName = "testService",
		//					QueueIdentifier = "helloWorldService"
		//				}
		//		});

		//	router.Start();


		//	Thread.Sleep(1000);

		//	router.Stop();

		//	receiveTask.Dispose();

		//	Thread.Sleep(1000);

		//	Assert.That(receivedCount, Is.EqualTo(1));
		//}

	}
}