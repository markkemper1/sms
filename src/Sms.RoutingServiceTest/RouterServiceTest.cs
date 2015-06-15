using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Sms.Messaging;
using Sms.Msmq;
using Sms.Router;

namespace Sms.RoutingServiceTest
{
	[TestFixture]
	public class RouterServiceTest
	{
		[SetUp]
		public void SetUp()
		{
			Defaults.MessagingFactories.Add(new MsmqFactory());
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

			FileBasedConfiguration loadFileBasedConfiguration = FileBasedConfiguration.LoadConfiguration();
			var router = new RouterService(loadFileBasedConfiguration);
			loadFileBasedConfiguration.Load(new List<ServiceEndpoint>()
                {
                    new ServiceEndpoint()
                        {
                            ProviderName = "msmq",
							MessageType = "testService",
                            QueueIdentifier = "helloWorldService"
                        }
                });

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

			var receiver1 = new ReceiveTask(SmsFactory.Receiver("msmq", "helloWorldService1"), message =>
			{
				receivedCount++;
				Console.WriteLine("Service 1 received a: " + message.Item.ToAddress);
				message.Success();
			});

			var receiver2 = new ReceiveTask(SmsFactory.Receiver("msmq", "helloWorldService2"), message =>
			{
				Console.WriteLine("Service 2 received a: " + message.Item.ToAddress);
				receivedCount++;
				message.Success();
			});


			receiver1.Start();
			receiver2.Start();

			FileBasedConfiguration loadFileBasedConfiguration = FileBasedConfiguration.LoadConfiguration();
			var router = new RouterService(loadFileBasedConfiguration);
			loadFileBasedConfiguration.Load(new List<ServiceEndpoint>()
			{
				new ServiceEndpoint()
				{
					ProviderName = "msmq",
					MessageType = "testService1",
					QueueIdentifier = "helloWorldService1"
				},
				new ServiceEndpoint()
				{
					ProviderName = "msmq",
					MessageType = "testService2",
					QueueIdentifier = "helloWorldService2"
				}
			});

			loadFileBasedConfiguration.AddMapping("testService", "testService1", "1");
			loadFileBasedConfiguration.AddMapping("testService", "testService2", "1");
			Thread.Sleep(1000);
			router.Start();

			Thread.Sleep(2000);

			RouterSink.Default.Send("testService", "Test me, hello?");

			Thread.Sleep(2000);

			router.Stop();

			receiver1.Dispose();
			receiver2.Dispose();

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