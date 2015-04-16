using System;
using System.Collections.Generic;
using System.Linq;
using Sms.Messaging;
using Sms.Routing;
using Sms.Services;

namespace Sms
{
    public class ServiceReceiverTask : IDisposable
    {
        private readonly IReceiver receiver;
        private readonly IServiceDefinitionRegistry registry;
        private readonly ISerializerFactory serializerFactory;
        private readonly IMessageSink errorSink;
        private ReceiveTask task;
        private readonly Dictionary<string, Registration> registeredHandlers = new Dictionary<string, Registration>();

        public ServiceReceiverTask(string provider, string queueName)
            : this(SmsFactory.Receiver(provider,queueName), null, null)
        {
        }

        public ServiceReceiverTask(IReceiver receiver, IServiceDefinitionRegistry registry = null, ISerializerFactory serializerFactory = null, IMessageSink errorSink = null)
        {
            this.receiver = receiver;
            this.errorSink = errorSink ??  SmsFactory.Sender(receiver.ProviderName, receiver.QueueName + "_errors");
            this.registry = registry ?? Defaults.ServiceDefinitionRegistry;
            this.serializerFactory = serializerFactory ?? Defaults.SerializerFactory;
            this.task = new ReceiveTask(receiver, HandleMessage);
        }

        public void Configure<T>(IServiceHandler<T> singletonHandler)
        {
            Register(() => singletonHandler);
        }

        public void Register<T>(IServiceHandler<T> singletonHandler)
        {
            Register(() => singletonHandler);
        }

        public void Register<T>(Func<IServiceHandler<T>> handlerFactory)
        {
            Register(typeof(T), handlerFactory);
        }

        public void Register<T>(Action<T> action)
        {
            Func<IServiceHandler<T>> handlerFactory = () => new GenericHandler<T>(action);
            Register(typeof(T), handlerFactory);
        }

        public void AutoRegister<T>(Func<Type, object> handlerFactory = null)
        {

            var allServices = typeof(T).Assembly.GetTypes().ToDictionary(x=>x,x=> GetServiceType(x, typeof(IServiceHandler<>))).Where(x=> x.Value != null);

            foreach (var requestType in allServices)
            {
				foreach (var typeHandled in requestType.Value)
				{
					this.Register(requestType.Key, typeHandled, handlerFactory ?? Activator.CreateInstance);
				}
            }

        }

        public void AutoRegister<T>( Type handlerIntefaceType, Func<Type, object> handlerFactory = null)
        {

            var allServices = typeof(T).Assembly.GetTypes().ToDictionary(x => x, x=>GetServiceType(x,handlerIntefaceType)).Where(x => x.Value != null);

            foreach (var requestType in allServices)
            {
	            foreach (var typeHandled in requestType.Value)
	            {
					this.Register(requestType.Key, typeHandled, handlerFactory ?? Activator.CreateInstance);
	            }
            }

        }

        private IEnumerable<Type> GetServiceType(Type type, Type handlerIntefaceType)
        {
            foreach (var i in type.GetInterfaces())
                if (i.IsGenericType && i.GetGenericTypeDefinition() == handlerIntefaceType)
                    yield return i.GetGenericArguments()[0]; ;
        }

        public virtual void Start()
        {
            //put errors back onto process queue
            using (var errorQueue = SmsFactory.Receiver(errorSink.ProviderName, errorSink.QueueName))
            {
                var errors = new List<MessageResult>();

                while (true)
                {
                    var error = errorQueue.Receive(TimeSpan.FromMilliseconds(0));

                    if (error == null)
                        break;

                    errors.Add(error);
                }

                using (var reQueue = SmsFactory.Sender(receiver.ProviderName, receiver.QueueName))
                {
                    foreach (var e in errors)
                    {
                        reQueue.Send(e.Item);
                        e.Success();
                    }
                }
            }

            this.task.Start();
        }

        public virtual Exception Stop()
        {
            return task.Stop();
        }

        public class GenericHandler<T> : IServiceHandler<T>
        {
            private readonly Action<T> action;

            public GenericHandler(Action<T> action)
            {
                this.action = action;
            }

            public void Process(T messageMessage)
            {
                action(messageMessage);
            }
        }

        public void Register(Type serviceType, Type requestType, Func<Type, object> containerFactory)
        {
            var serviceDefinition = registry.Get(requestType);

            this.registeredHandlers[serviceDefinition.RequestTypeName] = new Registration()
            {
                Serializer = serializerFactory.Get(serviceDefinition.Serializer),
                MessageType = requestType,
                HandlerFactory = new Func<dynamic>(()=> containerFactory(serviceType)),
                ServiceType = typeof(ServiceMessage<>).MakeGenericType(requestType)
            };
        }

        public void Register(Type type, dynamic handlerFactory)
        {
            var serviceDefinition = registry.Get(type);
            this.registeredHandlers[serviceDefinition.RequestTypeName] = new Registration()
             {
                 Serializer = serializerFactory.Get(serviceDefinition.Serializer),
                 MessageType = type,
                 HandlerFactory = handlerFactory,
                 ServiceType = typeof(ServiceMessage<>).MakeGenericType(type)
             };
        }

        private void HandleMessage(MessageResult messageResult)
        {
	        try
	        {
		        if (!messageResult.Item.Headers.ContainsKey(RouterSettings.ServiceNameHeaderKey))
			        throw new InvalidOperationException("Cannot receive messages without a service name header key");

		        var serviceName = messageResult.Item.Headers[RouterSettings.ServiceNameHeaderKey];

		        if (!registeredHandlers.ContainsKey(serviceName))
			        throw new InvalidOperationException(String.Format("Cannot receive the message type: {0} without registering a handler for the message type", serviceName));

		        var registration = registeredHandlers[serviceName];
		        var serializer = registration.Serializer;

		        dynamic item = serializer.Deserialize(registration.MessageType, messageResult.Item.Body);

		        try
		        {
			        registration.HandlerFactory().Process(item);
		        }
		        catch (Exception ex)
		        {
			        errorSink.Send(messageResult.Item);
			        Logger.Error("An unhanded exception occurred in the handler. Error: "  + ex + ", Service Name: "+ serviceName + " message has been placed on the error queue");
		        }
	        }
			catch (Exception ex)
			{
				errorSink.Send(messageResult.Item);
				Logger.Error("An general error occurred processing the message. Error: " + ex + " message has been placed on the error queue");
			}

	        messageResult.Success();
        }


        public class Registration
        {
            public Type MessageType { get; set; }
            public ISerializer Serializer { get; set; }
            public dynamic HandlerFactory { get; set; }
            public Type ServiceType{ get; set; }
            //public Action<dynamic> Process { get; set; }
        }

        //public class TypedMessageReceiver
        //{
        //    private readonly ServiceDefinitionRegistry registry;
        //    private readonly Dictionary<string, Registration> registeredHandlers = new Dictionary<string, Registration>();
        //    private readonly SerializerFactory serializerFactory;

        //    public class Registration
        //    {
        //        public Type MessageType { get; set; }
        //        public ISerializer Serializer { get; set; }
        //        public dynamic HandlerFactory { get; set; }
        //        public Type ServiceMessageType { get; set; }
        //    }


        //    public TypedMessageReceiver(ServiceDefinitionRegistry registry, SerializerFactory serializerFactory)
        //    {
        //        this.registry = registry;
        //        this.serializerFactory = serializerFactory;
        //    }

        //    public void Configure<T>(Func<T> handlerFactory)
        //    {
        //        Configure(typeof(T), handlerFactory);
        //    }

        //    public void Configure(Type type, dynamic handlerFactory)
        //    {
        //        var serviceDefinition = registry.Get(type);
        //       this.registeredHandlers[type] = new Registration()
        //        {
        //            Serializer = serializerFactory.Get(serviceDefinition.Serializer),
        //            MessageType = type,
        //            HandlerFactory = handlerFactory,
        //            ServiceMessageType = typeof (ServiceResult<>).MakeGenericType(type)
        //        };
        //    }
        public void Dispose()
        {
            this.task.Dispose();
        }
    }
}
