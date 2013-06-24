namespace Sms.Messaging
{
    public interface IMessagingFactory
    {
        string Name { get; }
        IMessageSender<SmsMessage> Sender(string queueName);
        IReceiver<SmsMessage> Receiver(string url);
    }
}