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
            var reciever = SmsFactory.Receiver("test","MyQueue");

            Assert.That(reciever,Is.AssignableTo<IMessageReciever>());
        }
    }
}