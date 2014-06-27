namespace Sms.Messaging
{
    public interface IMessagingFactory
    {
        string Name { get; }
        IMessageSink Sender(string queueName);
        IReceiver Receiver(string url);
    }
}