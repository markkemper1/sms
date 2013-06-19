//using System;
//using System.Collections.Generic;
//using ServiceStack.Redis;

//namespace Sms.Redis
//{
//    public class RedisMessageReciever : IMessageReciever
//    {
//        private readonly RedisClient client;

//        public RedisMessageReciever(string host, int port, string password, int db)
//        {
//            client = new RedisClient(host, port, password, db);
//        }

//        public void Dispose()
//        {
//            client.Dispose();
//        }


//        public void Subscribe(Func<SmsMessage, bool> action)
//        {
//            if (action == null) throw new ArgumentNullException("action");

//            var subscription = new Subscription(action);

//            while (true)
//            {
//                //Move from messages to processing list
//                var message = client.RPopLPush(Settings.MessagesKey, Settings.MessagesProcessingKey);

//                if (message == null)
//                    break;

//                bool result;
//                try
//                {
//                    result = subscription.Process(message);

//                }
//                catch (Exception)
//                {
//                    ProcessingFinished(message, false);
//                    throw;
//                }

//                ProcessingFinished(message, result);

//            }
//        }

//        private void ProcessingFinished(byte[] message, bool successful)
//        {
//            using (var trans = client.CreateTransaction())
//            {
//                //Remove from processing queue
//                client.LRem(Settings.MessagesProcessingKey, 1, message);

//                if (!successful)
//                    client.RPush(Settings.MessagesKey, message);
//                trans.Commit();
//            }
//        }

//        private class Subscription
//        {
//            public Func<SmsMessage, bool> Action { get; set; }

//            public Subscription(Func<SmsMessage, bool> action )
//            {
//                Action = action;
//            }

//            public HashSet<string> MessagesSeen { get; set; }

//            public bool Process(byte[] messageBytes)
//            {
//                var redisMessage = RedisMessage.DeSerialize(messageBytes);

//                if (MessagesSeen.Contains(redisMessage.Id))
//                {
//                    return false;
//                }

//                return Action(redisMessage.ToMessage());
//            }
//        }
//    }
//}