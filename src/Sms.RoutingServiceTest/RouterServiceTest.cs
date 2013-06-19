using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Sms.Messaging;
using Sms.Routing;
using Sms.RoutingService;

namespace Sms.RoutingServiceTest
{
    [TestFixture]
    public class RouterServiceTest
    {

        [SetUp]
        public void SetUp()
        {

        }


        [Test]
        public void should_send_messages()
        {
            int receivedCount = 0;

            var reciever = new RecieveTask<SmsMessage>(SmsFactory.Receiver("msmq", "helloWorldService"), message =>
                {
                    receivedCount++;
                    message.Success();
                });


            reciever.Start();

            var router = new RouterService();
            router.Config.Load(new List<ServiceEndpoint>()
                {
                    new ServiceEndpoint()
                        {
                            ProviderName = "msmq",
                            ServiceName = "testService",
                            QueueIdentifier = "helloWorldService"
                        }
                });

            router.Start();

            Thread.Sleep(1000);

            var watch = new Stopwatch();

            //warm up
            Router.Instance.Send("testService", "Test me, hello?");

            watch.Start();
            for (int i = 0; i < 1000; i++)
            {
                Router.Instance.Send("testService", "Test me, hello?");
            }
            watch.Stop();
            Console.WriteLine("Send 1000 in : " + watch.ElapsedMilliseconds);

            Thread.Sleep(1000);

            router.Stop();

            reciever.Dispose();

            Thread.Sleep(1000);

            Assert.That(receivedCount, Is.EqualTo(1001));
        }



        [Test]
        public void should_proxy_recieve_messages()
        {
            var sender = SmsFactory.Sender("msmq", "helloWorldService_Sending");

            int receivedCount = 0;

            sender.Send(new SmsMessage("helloWorldService_Sending", "hello there !"));


            var router = new RouterService();

            router.Config.Load(new List<ServiceEndpoint>()
                {
                    new ServiceEndpoint()
                        {
                            ProviderName = "msmq",
                            ServiceName = "testService_send",
                            QueueIdentifier = "helloWorldService_Sending"
                        }
                });

            router.Start();

            RecieveTask<SmsMessage> reciever = null;
            Thread.Sleep(1000);


            reciever = new RecieveTask<SmsMessage>(Router.Instance.Receiver("testService_send"), message =>
                 {
                     receivedCount++;
                     Console.WriteLine(message.Item.Body);
                     message.Success();
                 });

            reciever.Start();

            Thread.Sleep(1000);

            router.Stop();

            reciever.Dispose();

            Thread.Sleep(1000);

            Assert.That(receivedCount, Is.EqualTo(1));
        }


        [Test]
        public void should_put_unknown_messages_on_error_queue()
        {
            int receivedCount = 0;
            var reciever = new RecieveTask<SmsMessage>(SmsFactory.Receiver("msmq", RouterSettings.SendErrorQueueName), message =>
            {
                receivedCount++;
                message.Success();
            });

            reciever.Start();

            var router = new RouterService();

            router.Start();

            Thread.Sleep(1000);

            Stopwatch watch = new Stopwatch();

            //warm up
            Router.Instance.Send("testService", "Test me, hello?");

            Thread.Sleep(1000);

            router.Stop();

            reciever.Dispose();

            Thread.Sleep(1000);

            Assert.That(receivedCount, Is.EqualTo(1));



        }

        [Test]
        public void should_process_errors_on_startup()
        {
            int receivedCount = 0;

            var reciever = new RecieveTask<SmsMessage>(SmsFactory.Receiver("msmq", "helloWorldService"), message =>
            {
                receivedCount++;
                message.Success();
            });

            reciever.Start();

            var router = new RouterService();

            router.Start();

            Thread.Sleep(1000);

            //warm up
            Router.Instance.Send("testService", "Test me, hello?");

            Thread.Sleep(1000);

            router.Stop();

            Thread.Sleep(1000);

            Assert.That(receivedCount, Is.EqualTo(0));


            router = new RouterService();
            router.Config.Load(new List<ServiceEndpoint>()
                {
                    new ServiceEndpoint()
                        {
                            ProviderName = "msmq",
                            ServiceName = "testService",
                            QueueIdentifier = "helloWorldService"
                        }
                });

            router.Start();


            Thread.Sleep(1000);

            router.Stop();

            reciever.Dispose();

            Thread.Sleep(1000);

            Assert.That(receivedCount, Is.EqualTo(1));
        }

    }
}