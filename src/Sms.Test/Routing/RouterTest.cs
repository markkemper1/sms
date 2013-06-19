using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Sms.Messaging;

namespace Sms.Routing.Test
{
    [TestFixture]
    public class RouterTest
    {
        protected StubSender Sender { get; set; }
        protected StubSender NextMessage { get; set; }
        protected StubReceiver Receiver { get; set; }

        [SetUp]
        public void SetUp()
        {
            Sender = new StubSender();
            NextMessage = new StubSender();
        }

        [Test]
        public void send_should_send_message_via_broker_send_queue()
        {
            var target = new Router(Sender,NextMessage, Factory);

            target.Send("test", "hello world");

            Sender.Sent.Count.ShouldBe(1);
            Sender.Sent[0].ToAddress.ShouldBe("test");
            Sender.Sent[0].Body.ShouldBe("hello world");
        }

        [Test]
        public void send_should_add_service_name_in_header()
        {
            var target = new Router(Sender, NextMessage, Factory);

            target.Send("test", "hello world");

            Sender.Sent.Count.ShouldBe(1);
            Sender.Sent[0].Headers.Keys.Count.ShouldBe(1);
            Sender.Sent[0].Headers.Keys.First().ShouldBe("sms-router-serviceName");
            Sender.Sent[0].Headers.Values.First().ShouldBe("test");
        }

        [Test]
        public void receive_should_create_new_receiver()
        {
            var target = new Router(Sender, NextMessage, Factory);

            var reciever = target.Receiver("test");


            reciever.ShouldNotBe(null);
        }

        [Test]
        public void receive_should_tell_broker_to_send_next_message()
        {
            var target = new Router(Sender, NextMessage, Factory);

            var reciever = target.Receiver("test");
            var message = reciever.Receive();

            NextMessage.Sent.Count.ShouldBe(1);
            NextMessage.Sent[0].ToAddress.ShouldBe("test");


            reciever.ShouldBeTypeOf<BrokerProxingReciever>();

            NextMessage.Sent[0].Body.ShouldBe(((BrokerProxingReciever)reciever).RecieveQueueName);
        }

        [Test]
        public void receive_should_tell_broker_to_send_another_message_when_first_has_processed()
        {
            var target = new Router(Sender, NextMessage, Factory);

            var reciever = target.Receiver("test");

            var message1 = reciever.Receive();

            NextMessage.Sent.Count.ShouldBe(1);
            NextMessage.Sent[0].ToAddress.ShouldBe("test");
            message1.ShouldBe(null);

            Receiver.Messages.Enqueue(new SmsMessage("test", "xx"));
            var message2 = reciever.Receive();

            NextMessage.Sent.Count.ShouldBe(2);
            NextMessage.Sent[1].ToAddress.ShouldBe("test");
            message2.ShouldNotBe(null);

        }

        private IReciever<SmsMessage> Factory(string queueName)
        {
            Receiver = new StubReceiver();
            return Receiver;
        }
    }

    public class StubReceiver : IReciever<SmsMessage>
    {
        public int DisposedCount = 0;

        public void Dispose()
        {
            DisposedCount += 1;
        }

        public Result<SmsMessage> Receive(TimeSpan? timeout = null)
        {
            if (Messages.Count == 0) return null;
            var message = Messages.Peek();
            return new Result<SmsMessage>(message, b =>
            {
                                                         if (b) Messages.Dequeue();
            });
        }

        public Queue<SmsMessage>  Messages = new Queue<SmsMessage>();
    }

    public class StubSender : IMessageSender
    {
        public int DisposedCount = 0;

        public void Dispose()
        {
            DisposedCount += 1;
        }

        public void Send(SmsMessage smsMessage)
        {
            Sent.Add(smsMessage);
        }

        public List<SmsMessage> Sent = new List<SmsMessage>();
    }
}
