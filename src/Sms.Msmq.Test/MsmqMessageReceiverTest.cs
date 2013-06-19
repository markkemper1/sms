using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Sms.Messaging;

namespace Sms.Msmq.Test
{
    [TestFixture]
    public class MsmqMessageReceiverTest
    {
        [Test]
        public void should_send_test_message()
        {
            var queueName = @"SomeTestName";
            var sender = new MsmqMessageSender(queueName);

            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));

            int i = 0;

            var recever = new RecieveTask<SmsMessage>(new MsmqMessageReceiver(queueName), x =>
                {
                    i++;
                    x.Success();
                });

            recever.Start();

            Thread.Sleep(100);

            recever.Stop();

            recever.Dispose();

            Assert.That(i, Is.EqualTo(3));
        }



        [Test]
        public void should_leave_message_on_queue_if_exception()
        {


            var queueName = Guid.NewGuid().ToString();
            var sender = new MsmqMessageSender(queueName);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));

            stopwatch.Stop();
            Console.WriteLine("Send took: " + stopwatch.ElapsedMilliseconds + "ms");

            int i = 0;
             var receiver = new RecieveTask<SmsMessage>(new MsmqMessageReceiver(queueName), x =>
            {
                throw new ArgumentException("DIE");
            });

            receiver.Start();

            Thread.Sleep(100);

            receiver.Dispose();

            receiver = new RecieveTask<SmsMessage>(new MsmqMessageReceiver(queueName), x =>
             {
                i++;
                x.Success();
            });

            receiver.Start();

            Thread.Sleep(10);

            receiver.Dispose();

            Assert.That(i, Is.EqualTo(1));


        }

        [Test]
        public void should_continue_to_wait_for_messages()
        {


            var queueName =  Guid.NewGuid().ToString();
            var sender = new MsmqMessageSender(queueName);

            int i = 0;

            var receiver = new RecieveTask<SmsMessage>(new MsmqMessageReceiver(queueName), x =>
           {
                i++;
                x.Success();
            });

            receiver.Start();
           

            Thread.Sleep(10000);

            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));

            Thread.Sleep(1000);

            receiver.Dispose();

            Assert.That(i, Is.EqualTo(1));

        }

    }
}
