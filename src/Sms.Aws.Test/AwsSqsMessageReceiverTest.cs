using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Sms.Messaging;

namespace Sms.Aws.Test
{
    [TestFixture]
    public class AwsSqsMessageReceiverTest
    {

        [Test]
        public void should_send_receive_test_message()
        {
            var queueName = @"SomeTestName";


            var recever = new ReceiveTask(new AwsSqsMessageReceiver(AwsSqsFactory.ProviderName, queueName), x => x.Success());

            recever.Start();

            Thread.Sleep(1000);

            recever.Stop();


            var sender = new AwsSqsMessageSink(AwsSqsFactory.ProviderName, queueName);

            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));

            int i = 0;

            recever = new ReceiveTask(new AwsSqsMessageReceiver(AwsSqsFactory.ProviderName, queueName), x =>
            {
                i++;
                x.Success();
            });

            recever.Start();


            Thread.Sleep(1000);

            recever.Stop();

            recever.Dispose();

            Assert.That(i, Is.EqualTo(3));
        }



        [Test]
        public void should_leave_message_on_queue_if_exception()
        {
            var queueName = Guid.NewGuid().ToString();
            var sender = new AwsSqsMessageSink(AwsSqsFactory.ProviderName, queueName);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));

            stopwatch.Stop();
            Console.WriteLine("Send took: " + stopwatch.ElapsedMilliseconds + "ms");

            int i = 0;
            var receiver = new ReceiveTask(new AwsSqsMessageReceiver(AwsSqsFactory.ProviderName, queueName), x =>
            {
                throw new ArgumentException("DIE");
            });

            receiver.Start();

            Thread.Sleep(1000);

            receiver.Dispose();

            receiver = new ReceiveTask(new AwsSqsMessageReceiver(AwsSqsFactory.ProviderName, queueName), x =>
             {
                i++;
                x.Success();
            });

            receiver.Start();

            Thread.Sleep(1000);

            receiver.Dispose();

            Assert.That(i, Is.EqualTo(1));

        }

        [Test]
        public void should_continue_to_wait_for_messages()
        {


            var queueName =  Guid.NewGuid().ToString();
            var sender = new AwsSqsMessageSink(AwsSqsFactory.ProviderName, queueName);

            int i = 0;

            var receiver = new ReceiveTask(new AwsSqsMessageReceiver(AwsSqsFactory.ProviderName, queueName), x =>
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


        [Test, Explicit]
        public void long_running_test()
        {
            var queueName = @"SomeTestName";
            var sender = new AwsSqsMessageSink(AwsSqsFactory.ProviderName, queueName);
            bool send = true;
            int sentCount = 0;
            
            Logger.Setup.Debug(Console.WriteLine);

            var sentTask = Task.Factory.StartNew(() =>
                {
                    while (send)
                    {
                        sender.Send(new SmsMessage("http://test.sta1.com", "hello world"));
                        sentCount++;
                        Thread.Sleep(500);
                    }
                });


            int i = 0;

            var recever = new ReceiveTask(new AwsSqsMessageReceiver(AwsSqsFactory.ProviderName, queueName), x =>
            {
                i++;
                x.Success();
                Console.WriteLine("Sent: " + sentCount + " Received: " + i);
            });

            recever.Start();

            int runs = 90;
            while (runs > 0)
            {
                Thread.Sleep(100);
                runs--;

                if(recever.Status != TaskStatus.Running)
                    break;

                if (sentTask.Status != TaskStatus.Running)
                    break;
            }

            send = false;

            Thread.Sleep(2000);
            var ex1 = recever.Stop();

            recever.Dispose();

            Thread.Sleep(2000);

            if (ex1 != null)
                throw ex1;

            if (sentTask.Exception != null)
                throw sentTask.Exception;


            Assert.AreEqual(sentCount, i);
        }

    }
}
