﻿//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using Sms.RoutingService;
//using Sms.Services;

//namespace Sms.Test.Services
//{
//    [TestFixture]
//    public class ExchangeTest
//    {
//        RouterService service;

//        [SetUp]
//        public void SetUp()
//        {
//            service = new RoutingService.RouterService();

//            Task.Factory.StartNew(service.Start);
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            service.Stop();
//        }

//        [Test]
//        public void receive_be_able_to_send_and_receive()
//        {

//            var exchange = new Exchange();

//            service.Config.Load(new List<ServiceEndpoint>()
//                {
//                    new ServiceEndpoint()
//                        {
//                            ServiceName = "HelloWorldMessage",
//                            ProviderName = "msmq",
//                            QueueIdentifier = "HelloWorldMessages"
//                        }
//                });

//            exchange.Send(new HelloWorldMessage(){ Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm")});

//            Message<HelloWorldMessage> message = null;
//            exchange.Register<HelloWorldMessage>(x =>
//                {
//                    message = x;
//                    x.Processed(true);
//                });
                
//            exchange.Start();

//            Thread.Sleep(1000);

//            exchange.Stop();

//            Assert.That(message, Is.Not.Null);
//            Console.WriteLine(((HelloWorldMessage)message.Item).Text);

//        }

//        [Test]
//        public void receive_be_able_to_send_and_receive_with_long_wait()
//        {

//            var exchange = new Exchange();

//            service.Config.Load(new List<ServiceEndpoint>()
//                {
//                    new ServiceEndpoint()
//                        {
//                            ServiceName = "HelloWorldMessage",
//                            ProviderName = "msmq",
//                            QueueIdentifier = "HelloWorldMessages"
//                        }
//                });


//            int recieveCount = 0;
//            Message<HelloWorldMessage> message = null;
//            exchange.Register<HelloWorldMessage>(x =>
//            {
//                message = x;
//                recieveCount++;
//                x.Processed(true);
//            });

//            exchange.Start();

//            Thread.Sleep(20000);

//            exchange.Send(new HelloWorldMessage() { Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm") });


//            Thread.Sleep(20000);

//            exchange.Send(new HelloWorldMessage() { Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm") });

//            Thread.Sleep(20000);

//            exchange.Send(new HelloWorldMessage() { Text = "Hi there. Its " + DateTime.Now.ToString("HH:mm") });

//            Thread.Sleep(2000);
//            exchange.Stop();

//            Assert.That(message, Is.Not.Null);

//            Assert.That(recieveCount, Is.EqualTo(3));
//            Console.WriteLine(((HelloWorldMessage)message.Item).Text);

//        }



//        public class HelloWorldMessage
//        {
//            public string Text { get; set; }
//        }
//    }

   
//}
