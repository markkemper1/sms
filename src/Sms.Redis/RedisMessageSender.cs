using ServiceStack.Redis;
using Sms.Messaging;

namespace Sms.Redis
{
    public class RedisMessageSender : IMessageSender
    {
        private readonly RedisNativeClient client;

        public RedisMessageSender(string host, int port, string password, int db)
        {
            client = new RedisNativeClient(host, port, password, db);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public void Send(SmsMessage smsMessage)
        {
            var m = RedisMessage.Create(smsMessage);
            client.LPush(Settings.MessagesKey, m.Serialize());
        }
    }
}