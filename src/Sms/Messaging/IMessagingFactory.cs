namespace Sms.Messaging
{
    public interface IMessagingFactory
    {
        string Name { get; }
        IMessageSender Sender(string queueName);
        IReciever<SmsMessage> Reciever(string url);
    }
}