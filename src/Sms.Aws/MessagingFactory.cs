//using Sms.Messaging;

//namespace Sms.Aws
//{
//    public class AwsSqsFactory : IMessagingFactory
//    {
//        public const string ProviderName = "awssqs";

//        public string Name { get { return ProviderName; } }

//        public IMessageSender<SmsMessage> Sender(string queueName)
//        {
//            return new AwsSqsMessageSender(queueName);
//        }

//        public IReceiver<SmsMessage> Receiver(string queueName)
//        {
//            return new AwsSqsMessageReceiver(queueName);
//        }
//    }
//}