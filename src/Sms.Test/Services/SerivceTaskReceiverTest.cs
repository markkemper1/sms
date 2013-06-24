using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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
            router = new RoutingService.RouterService();
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
            var router1 = RouterFactory.Build();
            var exchange = new Exchange(router1);

            router.Config.Load(new List<ServiceEndpoint>()
                {
                    new ServiceEndpoint()
                        {
                            ServiceName = "HelloWorldMessage1",
                            ProviderName = "msmq",
                            QueueIdentifier = "HelloWorldMessagesTest1"
                        },
                         new ServiceEndpoint()
                        {
                            ServiceName = "HelloWorldMessage2",
                            ProviderName = "msmq",
                            QueueIdentifier = "HelloWorldMessagesTest2"
                        }
                });

            exchange.Send(new HelloWorldMessage1() { Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm") });
            exchange.Send(new HelloWorldMessage2() { Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm") });


            bool helloWorld1 = false, helloWorld2 = false;


            exchange.Register<HelloWorldMessage1>(message => { helloWorld1 = true; message.Processed(true); });
            exchange.Register<HelloWorldMessage2>(message => { helloWorld2 = true; message.Processed(true); });


            exchange.Start();

            Thread.Sleep(1000);

            var errors = exchange.Stop();

            foreach (var e in errors)
                throw e;
            exchange.Dispose();

            Assert.That(helloWorld1, Is.True);
            Assert.That(helloWorld2, Is.True);

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
