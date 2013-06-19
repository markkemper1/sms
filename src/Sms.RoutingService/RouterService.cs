using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
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
        private RecieveTask<SmsMessage> sendQueueTask, nextMessageQueueTask;

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

            log.Info("RouterService logging setup...");
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
                    var errorErrors = new List<Result<SmsMessage>>();
                    var errorSuccess = new List<Result<SmsMessage>>();

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
            sendQueueTask = new RecieveTask<SmsMessage>(SmsFactory.Receiver(RouterSettings.ProviderName, RouterSettings.SendQueueName),
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
            nextMessageQueueTask = new RecieveTask<SmsMessage>(SmsFactory.Receiver(RouterSettings.ProviderName, RouterSettings.NextMessageQueueName), (message) =>
            {
                try
                {
                    string queueIdentifier = message.Item.Body;

                    var configInfo = Config.Get(message.Item.ToAddress);

                    if (!receivers.ContainsKey(queueIdentifier))
                    {
                        var messageReciever = SmsFactory.Receiver(configInfo.ProviderName, configInfo.QueueIdentifier);
                        var toQueue = SmsFactory.Sender(RouterSettings.ProviderName, queueIdentifier);

                        receivers[queueIdentifier] = new PipingMessageReciever(messageReciever, toQueue, TimeSpan.FromMilliseconds(10));
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

            Task.Factory.StartNew(() =>{
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
        }

        private static readonly ServiceEndpoint ErrorConfig = new ServiceEndpoint()
            {
                ProviderName = RouterSettings.ProviderName,
                ServiceName = "Router",
                QueueIdentifier = RouterSettings.SendErrorQueueName
            };

        private readonly ConcurrentDictionary<string, PipingMessageReciever> receivers = new ConcurrentDictionary<string, PipingMessageReciever>();

        private bool stop;

        public void Stop()
        {
            this.stop = true;

            if (sendQueueTask != null)
                sendQueueTask.Dispose();

            if (nextMessageQueueTask != null)
                nextMessageQueueTask.Dispose();
        }
    }

    public class PipingMessageReciever : IDisposable
    {
        private readonly TimeSpan timeSpan;
        public IReciever<SmsMessage> Reciever { get; set; }
        public IMessageSender ToQueue { get; set; }

        public PipingMessageReciever(IReciever<SmsMessage> reciever, IMessageSender toQueue, TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
            Reciever = reciever;
            ToQueue = toQueue;
        }
        public bool IsActive { get; set; }

        public bool CheckOne()
        {
            if (!IsActive) return false;

            var message = Reciever.Receive(timeSpan);

            if (message != null)
            {
                ToQueue.Send(message.Item);
                message.Success();
                IsActive = false;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Reciever.Dispose();
            ToQueue.Dispose();
        }
    }
}