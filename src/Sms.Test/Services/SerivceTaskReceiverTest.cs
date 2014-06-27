using System;
using System.Collections.Generic;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Sms.Msmq;
using Sms.Router;
using Sms.Routing;
using Sms.RoutingService;

namespace Sms.Services.Test
{
    [TestFixture]
    public class SerivceTaskReceiverTest
    {
        RouterService router;

        [SetUp]
        public void SetUp()
        {
            router = new RouterService();
            Task.Factory.StartNew(router.Start);
        }

        [TearDown]
        public void TearDown()
        {
            router.Stop();
        }


        [Test]
        public void receive_be_able_to_send_and_receive_different_message_types()
        {
            string queueName = "HelloWorlds";

            try
            {
                var messageQueue = new MessageQueue(@".\Private$\" + queueName);
                messageQueue.Purge();
                messageQueue.Dispose();
            }
            catch
            {
            }
        

            router.Config.Load(new List<ServiceEndpoint>()
                {
                    new ServiceEndpoint()
                        {
                            MessageType = "HelloWorldMessage1",
                            ProviderName = "msmq",
                            QueueIdentifier = queueName
                        },
                         new ServiceEndpoint()
                        {
                            MessageType = "HelloWorldMessage2",
                            ProviderName = "msmq",
                            QueueIdentifier =queueName
                        }
                });

            RouterSink.Default.Send(new HelloWorldMessage1() { Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm") });
            RouterSink.Default.Send(new HelloWorldMessage2() { Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm") });

            bool helloWorld1 = false, helloWorld2 = false;

            Thread.Sleep(100);

            using (var reciever = new ServiceReceiverTask(new MsmqMessageReceiver(MsmqFactory.ProviderName, queueName)))
            {
                reciever.Register<HelloWorldMessage1>(message =>
                {
                    helloWorld1 = true;
                });
                reciever.Register<HelloWorldMessage2>(message =>
                {
                    helloWorld2 = true;
                });

                reciever.Start();

                Thread.Sleep(1000);

                var error = reciever.Stop();

                if (error != null)
                {
                    throw error;
                }

                Assert.That(helloWorld1, Is.True);
                Assert.That(helloWorld2, Is.True);
            }
        }

        public class HelloWorldMessage1
        {
            public string Text { get; set; }
        }

        public class HelloWorldMessage2
        {
            public string Text { get; set; }
        }
    }

   
}
