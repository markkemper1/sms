using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Sms.Messaging;
using Sms.Services;
using IMessageSink = Sms.Messaging.IMessageSink;

namespace Sms.Routing.Test
{
    [TestFixture]
    public class RouterTest
    {
        protected StubSender Sender { get; set; }

        [SetUp]
        public void SetUp()
        {
            Sender = new StubSender();
        }

        private RouterSink CreateRouter()
        {
            return new RouterSink(Sender, new ServiceDefinitionRegistry(), new SerializerFactory());
        }

        [Test]
        public void send_should_send_message_via_broker_send_queue()
        {
            var target = CreateRouter();

            target.Send(new SmsMessage( "test", "hello world"));

            Sender.Sent.Count.ShouldBe(1);
            Sender.Sent[0].ToAddress.ShouldBe("test");
            Sender.Sent[0].Body.ShouldBe("hello world");
        }

        [Test]
        public void send_should_add_service_name_in_header()
        {
            var target = CreateRouter();

            target.Send(new SmsMessage("test", "hello world"));

            Sender.Sent.Count.ShouldBe(1);
            Sender.Sent[0].Headers.Keys.Count.ShouldBe(1);
            Sender.Sent[0].Headers.Keys.First().ShouldBe("sms-router-serviceName");
            Sender.Sent[0].Headers.Values.First().ShouldBe("test");
        }

    }

    public class StubSender : IMessageSink
    {
        public int DisposedCount = 0;

        public void Dispose()
        {
            DisposedCount += 1;
        }

        public string ProviderName { get; private set; }
        public string QueueName { get; private set; }

        public void Send(SmsMessage smsMessage)
        {
            Sent.Add(smsMessage);
        }

        public List<SmsMessage> Sent = new List<SmsMessage>();
    }
}
