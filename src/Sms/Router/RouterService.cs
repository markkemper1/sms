using System;
using System.Collections.Generic;
using System.Linq;
using Sms.Messaging;
using Sms.Routing;

namespace Sms.Router
{
	public class RouterService
	{
		public const String ConfigureServiceEndpointAddress = "SmsRouter.Configure.Endpoint.Add";
		public const String ConfigureServiceEndpointAddressRemove = "SmsRouter.Configure.Endpoint.Remove";
		public const String ConfigureServiceMappingAddress= "SmsRouter.Configure.Mapping.Add";
		public const String ConfigureServiceMappingAddressRemove = "SmsRouter.Configure.Mapping.Remove";

		private ReceiveTask sendQueueTask;
		//private Task pipeMessages ;

		public RouterService(IRouterConfiguration configuration)
		{
			this.Config = configuration;
		}

		public IRouterConfiguration Config { get; private set; }

		public void Start()
		{

			//Process Errors
			using (var errorQueue = SmsFactory.Receiver(RouterSettings.ProviderName, RouterSettings.SendErrorQueueName))
			{
				var errorErrors = new List<MessageResult>();
				var errorSuccess = new List<MessageResult>();

				while (true)
				{
					var error = errorQueue.Receive(TimeSpan.FromMilliseconds(0));

					if (error == null)
						break;

					if (Config.Get(error.Item.ToAddress).Any())
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
			sendQueueTask = new ReceiveTask(SmsFactory.Receiver(RouterSettings.ProviderName, RouterSettings.SendQueueName),
													message =>
													{
														try
														{
															if (ProcessConfigurationMessages(message)) return;

															var configInfoList = Config.Get(message.Item.ToAddress);

															bool atLeastOne = false;

															foreach (var configInfo in configInfoList)
															{
																using (var sender = SmsFactory.Sender(configInfo.ProviderName, configInfo.QueueIdentifier))
																{
																	var headers = new Dictionary<string, string>(message.Item.Headers);
																	headers[RouterSettings.ServiceNameHeaderKey] = configInfo.MessageType;
																	sender.Send(new SmsMessage(configInfo.MessageType, message.Item.Body,headers));
																	message.Success();
																}
																atLeastOne = true;
															}

															if (!atLeastOne)
															{
																using (var sender = SmsFactory.Sender(ErrorConfig.ProviderName, ErrorConfig.QueueIdentifier))
																{
																	sender.Send(message.Item);
																	message.Success();
																}
															}
														}
														catch (Exception ex)
														{
															Logger.Fatal("Exception", ex);
															throw;
														}

													});
			sendQueueTask.Start();

		}

		private bool ProcessConfigurationMessages(MessageResult message)
		{
			if (message.Item.ToAddress == ConfigureServiceEndpointAddress)
			{
				Config.Add(new ServiceEndpoint()
				{
					MessageType = message.Item.Headers["MessageType"],
					ProviderName = message.Item.Headers["ProviderName"],
					QueueIdentifier = message.Item.Headers["QueueIdentifier"]
				});

				message.Success();
				return true;
			}

			if (message.Item.ToAddress == ConfigureServiceEndpointAddressRemove)
			{
				Config.Remove(new ServiceEndpoint()
				{
					MessageType = message.Item.Headers["MessageType"],
					ProviderName = message.Item.Headers["ProviderName"],
					QueueIdentifier = message.Item.Headers["QueueIdentifier"]
				});

				message.Success();
				return true;
			}

			if (message.Item.ToAddress == ConfigureServiceMappingAddress)
			{
				Config.AddMapping(message.Item.Headers["FromType"], message.Item.Headers["ToType"]);
				message.Success();
				return true;
			}

			if (message.Item.ToAddress == ConfigureServiceMappingAddressRemove)
			{
				Config.RemoveMapping(message.Item.Headers["FromType"], message.Item.Headers["ToType"]);
				message.Success();
				return true;
			}
			return false;
		}


		private static readonly ServiceEndpoint ErrorConfig = new ServiceEndpoint()
			{
				ProviderName = RouterSettings.ProviderName,
				MessageType = "Router",
				QueueIdentifier = RouterSettings.SendErrorQueueName
			};

		//        private readonly ConcurrentDictionary<string, ProxyMessageReceiver> receivers = new ConcurrentDictionary<string, ProxyMessageReceiver>();


		public Exception Stop()
		{
			Exception ex = null;
			var exceptions = new List<Exception>();

			if (sendQueueTask != null)
			{
				ex = sendQueueTask.Stop();

				if (ex != null) Logger.Fatal("Send Queue Ex:", ex);

				sendQueueTask.Dispose();
			}

			if (ex != null)
				exceptions.Add(ex);

			//ex = null;

			//if (nextMessageQueueTask != null)
			//{
			//    ex = nextMessageQueueTask.Stop();
			//    log.Fatal("Next Message Queue Ex:", ex);
			//    nextMessageQueueTask.Dispose();
			//}

			//if (ex != null)
			//    exceptions.Add(ex);

			//ex = null;

			//try
			//{
			//    Task.WaitAll(pipeMessages);
			//}
			//catch (AggregateException ae)
			//{
			//    log.Fatal("Piping Task:", ex);
			//    exceptions.Add(ae);
			//}
			return new AggregateException(exceptions);
		}
	}

	//public class ProxyMessageReceiver : IDisposable
	//{
	//    private readonly TimeSpan timeSpan;
	//    public IReceiver Receiver { get; set; }
	//    public IMessageSink ToQueue { get; set; }

	//    public ProxyMessageReceiver(IReceiver receiver, IMessageSink toQueue, TimeSpan timeSpan)
	//    {
	//        this.timeSpan = timeSpan;
	//        Receiver = receiver;
	//        ToQueue = toQueue;
	//    }
	//    public bool IsActive { get; set; }

	//    public bool CheckOne()
	//    {
	//        Logger.Debug("ProxyMessageReceiver: Checking for message on {0},", Receiver.QueueName);

	//        if (!IsActive)
	//        {
	//            Logger.Debug("ProxyMessageReceiver: Not active. IsActive: {0}", IsActive);
	//            return false;
	//        }

	//        var message = Receiver.Receive(timeSpan);

	//        if (message != null)
	//        {
	//            Logger.Debug("ProxyMessageReceiver: Received Message on Queue: {0}", Receiver.QueueName);
	//            ToQueue.Send(message.Item);
	//            message.Success();
	//            IsActive = false;
	//            return true;
	//        }

	//        Logger.Debug("PipingMessageReceiver: No message received. QueueName: {0}", Receiver.QueueName);

	//        return false;
	//    }

	//    public void Dispose()
	//    {
	//        Receiver.Dispose();
	//        ToQueue.Dispose();
	//    }
	//}
}