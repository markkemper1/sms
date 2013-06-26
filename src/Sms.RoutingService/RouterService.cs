using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sms.Messaging;
using Sms.Routing;
using log4net;
using log4net.Config;

namespace Sms.RoutingService
{
    public class RouterService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RouterService));
        private static readonly ILog smsLog = LogManager.GetLogger("SMS");
        private ReceiveTask<SmsMessage> sendQueueTask, nextMessageQueueTask;
        private Task pipeMessages ;

        public RouterService()
        {
            Config = new Configuration();
            SetUpLogging();
            LoadConfiguration();
        }

        public Configuration Config { get; private set; }

        private void SetUpLogging()
        {
            var configFile = new FileInfo("log4net.config");

            if (!configFile.Exists)
                throw new ApplicationException("Log4net config file doesn't exist!. No you can't run without it!");

            XmlConfigurator.ConfigureAndWatch(configFile);

            log.Info("RouterService logging setup..");

            Logger.Setup.Info(s => smsLog.Info(s));
            Logger.Setup.Warn(s => smsLog.Warn(s));
            Logger.Setup.Error(s => smsLog.Error(s));
            Logger.Setup.Fatal(s => smsLog.Fatal(s));
        }

        private void LoadConfiguration()
        {
            var section = (NameValueCollection)ConfigurationManager.GetSection("ServiceConfiguration");

            if (section == null)
            {
                log.Error("Configuration could not be read");
                return;
            }

            if (section.Keys.Count == 0)
            {
                log.Error("Configuration does not contain any services, configuration key count == 0");
                return;
            }

            var services = new List<ServiceEndpoint>();

            foreach (string key in section.Keys)
            {
                string value = section[key];

                var valueSplit = value.Split(new[] { "://" }, 2, StringSplitOptions.RemoveEmptyEntries);

                string provider = valueSplit[0];
                string queueName = valueSplit[1];

                services.Add(new ServiceEndpoint()
                    {
                        ServiceName = key,
                        ProviderName = provider,
                        QueueIdentifier = queueName
                    });
            }

            Config.Load(services);
        }

        public void Start()
        {

            //Process Errors
            using (var errorQueue = SmsFactory.Receiver(RouterSettings.ProviderName, RouterSettings.SendErrorQueueName))
            {
                    var errorErrors = new List<Message<SmsMessage>>();
                    var errorSuccess = new List<Message<SmsMessage>>();

                while (true)
                {
                    var error = errorQueue.Receive(TimeSpan.FromMilliseconds(0));

                    if (error == null)
                        break;

                    if (Config.IsKnown(error.Item.ToAddress))
                        errorSuccess.Add(error);
                    else
                        errorErrors.Add(error);
                }

                using (var reQueue = SmsFactory.Sender(RouterSettings.ProviderName, RouterSettings.SendQueueName))
                {
                    foreach (var e in errorErrors)
                        e.Failed();

                    foreach (var e in errorSuccess)
                    {
                        reQueue.Send(e.Item);
                        e.Success();
                    }
                }
            }


            //Listen on the send Queue and forward messages to the configured service.
            sendQueueTask = new ReceiveTask<SmsMessage>(SmsFactory.Receiver(RouterSettings.ProviderName, RouterSettings.SendQueueName),
                                                    message =>
                                                        {
                                                            try
                                                            {
                                                                var configInfo = Config.IsKnown(message.Item.ToAddress) ? Config.Get(message.Item.ToAddress) : ErrorConfig;

                                                                using (var sender = SmsFactory.Sender(configInfo.ProviderName, configInfo.QueueIdentifier))
                                                                {
                                                                    sender.Send(message.Item);
                                                                    message.Success();
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                log.Fatal("Exception", ex);
                                                                throw;
                                                            }

                                                        });
            sendQueueTask.Start();

            //Listen for the next message queue, Then setup a receive (if needed) and forward onto the unique queue provided..
            nextMessageQueueTask = new ReceiveTask<SmsMessage>(SmsFactory.Receiver(RouterSettings.ProviderName, RouterSettings.NextMessageQueueName), (message) =>
            {
                try
                {
                    string queueIdentifier = RouterSettings.ReceiveMessageQueueNamePrefix + message.Item.Headers[RouterSettings.ServiceNameHeaderKey];

                    var configInfo = Config.Get(message.Item.ToAddress);

                    if (!receivers.ContainsKey(queueIdentifier))
                    {
                        var receiver = SmsFactory.Receiver(configInfo.ProviderName, configInfo.QueueIdentifier);
                        var toQueue = SmsFactory.Sender(RouterSettings.ProviderName, queueIdentifier);

                        receivers[queueIdentifier] = new PipingMessageReceiver(receiver, toQueue, TimeSpan.FromMilliseconds(10));
                    }

                    receivers[queueIdentifier].IsActive = true;

                    message.Success();
                }
                catch (Exception ex)
                {
                    log.Fatal("Exception", ex);
                    throw;
                }
            });

            nextMessageQueueTask.Start();

            pipeMessages = Task.Factory.StartNew(() =>{
                                           try
                                           {
                                               while (!stop)
                                               {
                                                   bool hasWork = false;
                                                   foreach (var item in receivers)
                                                   {
                                                       hasWork = item.Value.CheckOne() || hasWork;
                                                   }

                                                   Thread.Sleep(hasWork ? 10 : 100);
                                               }
                                           }
                                           catch (Exception ex)
                                           {
                                               log.Fatal("Exception", ex);
                                               throw;
                                           }
            });

            while (pipeMessages.Status == TaskStatus.WaitingToRun || pipeMessages.Status == TaskStatus.Created || pipeMessages.Status == TaskStatus.WaitingForActivation || 
                   nextMessageQueueTask.Status == TaskStatus.WaitingToRun || nextMessageQueueTask.Status == TaskStatus.Created || nextMessageQueueTask.Status == TaskStatus.WaitingForActivation || 
                   sendQueueTask.Status == TaskStatus.WaitingToRun || sendQueueTask.Status == TaskStatus.Created || sendQueueTask.Status == TaskStatus.WaitingForActivation
            )
            {
                var message = String.Format("Something isn't running. 1. pipeMessages:  {0}, nextMessageQueue: {1}, sendQueue: {2} ", pipeMessages.Status.ToString(), nextMessageQueueTask.Status.ToString(), sendQueueTask.Status.ToString());
                log.Info(message);

                Thread.Sleep(1000);
            }



            if (pipeMessages.Status != TaskStatus.Running || nextMessageQueueTask.Status != TaskStatus.Running || sendQueueTask.Status != TaskStatus.Running)
            {
                var message = String.Format("Something isn't running. 1. pipeMessages:  {0}, nextMessageQueue: {1}, sendQueue: {2} ", pipeMessages.Status.ToString(), nextMessageQueueTask.Status.ToString(), sendQueueTask.Status.ToString());
                var exception = this.Stop();
                throw new Exception(message , exception);
            }
        }

        

        private static readonly ServiceEndpoint ErrorConfig = new ServiceEndpoint()
            {
                ProviderName = RouterSettings.ProviderName,
                ServiceName = "Router",
                QueueIdentifier = RouterSettings.SendErrorQueueName
            };

        private readonly ConcurrentDictionary<string, PipingMessageReceiver> receivers = new ConcurrentDictionary<string, PipingMessageReceiver>();

        private bool stop;

        public Exception Stop()
        {
            this.stop = true;

            Exception ex = null;
            var exceptions = new List<Exception>();

            if (sendQueueTask != null)
            {
                ex = sendQueueTask.Stop();
                log.Fatal("Send Queue Ex:", ex);
                sendQueueTask.Dispose();
            }

            if(ex != null)
                exceptions.Add(ex);

            ex = null;

            if (nextMessageQueueTask != null)
            {
                ex = nextMessageQueueTask.Stop();
                log.Fatal("Next Message Queue Ex:", ex);
                nextMessageQueueTask.Dispose();
            }

            if (ex != null)
                exceptions.Add(ex);

            ex = null;

            try
            {
                Task.WaitAll(pipeMessages);
            }
            catch (AggregateException ae)
            {
                log.Fatal("Piping Task:", ex);
                exceptions.Add(ae);
            }
            return new AggregateException(exceptions);
        }
    }

    public class PipingMessageReceiver : IDisposable
    {
        private readonly TimeSpan timeSpan;
        public IReceiver<SmsMessage> Receiver { get; set; }
        public IMessageSender<SmsMessage> ToQueue { get; set; }
        private DateTime lastReceiveTried = new DateTime();

        public PipingMessageReceiver(IReceiver<SmsMessage> receiver, IMessageSender<SmsMessage> toQueue, TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
            Receiver = receiver;
            ToQueue = toQueue;
        }
        public bool IsActive { get; set; }

        public bool CheckOne()
        {
            Logger.Debug("PipingMessageReceiver: Checking for message on {0},", Receiver.QueueName);

            if (!IsActive && DateTime.UtcNow.Subtract(lastReceiveTried).TotalSeconds < 30)
            {
                Logger.Debug("PipingMessageReceiver: Not active. IsActive: {0}, lastReceived seconds: {1},", IsActive, DateTime.UtcNow.Subtract(lastReceiveTried).TotalSeconds);
                return false;
            }

            var message = Receiver.Receive(timeSpan);

            
            lastReceiveTried = DateTime.UtcNow;

            if (message != null)
            {
                Logger.Debug("PipingMessageReceiver: Received Message on Queue: {0}", Receiver.QueueName);
                ToQueue.Send(message.Item);
                message.Success();
                IsActive = false;
                return true;
            }

            Logger.Debug("PipingMessageReceiver: No message received. QueueName: {0}", Receiver.QueueName);

            return false;
        }

        public void Dispose()
        {
            Receiver.Dispose();
            ToQueue.Dispose();
        }
    }
}