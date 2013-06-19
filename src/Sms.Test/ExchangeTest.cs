using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Sms.RoutingService;

namespace Sms.Services.Test
{
    [TestFixture]
    public class ExchangeTest
    {
        RouterService service;

        [SetUp]
        public void SetUp()
        {
            service = new RoutingService.RouterService();

            Task.Factory.StartNew(service.Start);
        }

        [TearDown]
        public void TearDown()
        {
            service.Stop();
        }


        [Test]
        public void receive_be_able_to_send_and_receive()
        {

            var exchange = new Exchange();

            service.Config.Load(new List<ServiceEndpoint>()
                {
                    new ServiceEndpoint()
                        {
                            ServiceName = "HelloWorldMessage",
                            ProviderName = "msmq",
                            QueueIdentifier = "HelloWorldMessages"
                        }
                });

            exchange.Send(new HelloWorldMessage(){ Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm")});

            var message = exchange.Receive<HelloWorldMessage>(TimeSpan.FromSeconds(5));

            exchange.Processed(true);

            Assert.That(message, Is.Not.Null);
            Console.WriteLine(message.Text);

        }

        public class HelloWorldMessage
        {
            public string Text { get; set; }
        }
    }

   
}
