using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Sms.RoutingService;
using Sms.Services;

namespace Sms.Test.Services
{
    [TestFixture]
    public class AutoConfigureServiceBaseTest
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
        public void should_configure_Services()
        {
            var exchange = new Exchange();

            router.Config.Load(new List<ServiceEndpoint>()
                {
                    new ServiceEndpoint()
                        {
                            ServiceName = "HelloWorldAuto",
                            ProviderName = "msmq",
                            QueueIdentifier = "HelloWorldAuto__1"
                        },
                });

            exchange.Send(new HelloWorldAuto(){ Text = "Ping"});


            var auto = new AutoTestService();

            auto.Start();

            Thread.Sleep(1000);

            auto.Stop();
            
        }

        public class AutoTestService : AutoConfigureServiceBase
        {
        }

        public class HelloWorldAuto
        {
            public string Text { get; set; }
        }

        public class TestServiceReceiver : ServiceReceiver<HelloWorldAuto>
        {
            public override void Process(Message<HelloWorldAuto> message)
            {
                Console.WriteLine("I got 1!");
                message.Processed(true);
            }
        }
    }
}
