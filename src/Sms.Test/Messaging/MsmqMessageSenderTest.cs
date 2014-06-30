using NUnit.Framework;
using Sms.Messaging;

namespace Sms.Test
{
    [TestFixture]
    public class MsmqMessageSenderTest 
    {
        [Test]
        public void should_create_find_and_create_factory()
        {
            var receiver = SmsFactory.Receiver("msmq","MyQueue");

            Assert.That(receiver, Is.AssignableTo<IReceiver>());
        }
    }
}