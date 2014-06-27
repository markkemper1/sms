using ServiceStack.Redis;
using Sms.Messaging;

namespace Sms.Redis
{
    public class RedisMessageSink : IMessageSink
    {
        private readonly RedisNativeClient client;

        public RedisMessageSink(string host, int port, string password, int db)
        {
            client = new RedisNativeClient(host, port, password, db);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public string ProviderName { get; private set; }

        public string QueueName { get; private set; }

        public void Send(SmsMessage smsMessage)
        {
            var m = RedisMessage.Create(smsMessage);
            client.LPush(Settings.MessagesKey, m.Serialize());
        }
    }
}