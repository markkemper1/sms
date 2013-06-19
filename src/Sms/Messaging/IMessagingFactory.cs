namespace Sms.Messaging
{
    public interface IMessagingFactory
    {
        string Name { get; }
        IMessageSender Sender(string queueName);
        IMessageReciever Reciever(string url);
    }
}