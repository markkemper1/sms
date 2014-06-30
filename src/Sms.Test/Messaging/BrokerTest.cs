using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sms.Messaging;

namespace Sms.Test
{
    [TestFixture]
    public class BrokerTest
    {
        [Test]
        public void should_be_able_()
        {
            var receiver = SmsFactory.Receiver("msmq", "MyQueue");

            Assert.That(receiver, Is.AssignableTo<IReceiver>());
        }
    }
}
