using System;
using System.Collections.Generic;
using System.Linq;
using Sms.Internals;
using Sms.Messaging;
using Sms.Routing;

namespace Sms.Services
{
    public class Exchange : IMessageSink, IDisposable
    {
        private readonly ServiceDefinitionRegistry registry;
        private readonly SerializerFactory serializerFactory;
        private readonly IRouter router;
        private readonly List<ServiceReceiverTask> tasks = new List<ServiceReceiverTask>(); 

        public Exchange(IRouter router = null, ServiceDefinitionRegistry register = null, SerializerFactory serializerFactory = null)
        {
            this.registry = register ?? new ServiceDefinitionRegistry();
            this.serializerFactory = serializerFactory ?? new SerializerFactory();
            this.router = router ?? RouterFactory.Build(); ;
        }

        public void Send<T>(T request) where T : class, new()
        {
            var config = registry.Get<T>();

            var serializer = serializerFactory.Get(config.Serializer);

            router.Send(config.ServiceName, serializer.Serialize(request));
        }


        public void Send(SmsMessage message)
        {
            router.Send(message.ToAddress, message.Body, message.Headers);
        }

        public SmsMessage ToMessage<T>(T request) where T : class, new()
        {
            var config = registry.Get<T>();

            var serializer = serializerFactory.Get(config.Serializer);

            return new SmsMessage(config.ServiceName, serializer.Serialize(request));
        }

        public void Register(Type serviceType, Action<Message<object>> handler)
        {
            var receiver = CreateReceiverTask(serviceType);
            receiver.Register(serviceType, handler);
            tasks.Add(receiver);
        }

        public void Register<T>(Action<Message<T>> handler)
        {
            ServiceReciever<T> handlerClass = ServiceReciever<T>.Create(handler);

            var receiver = CreateReceiverTask(handlerClass.MessageItemType);
            receiver.Register(handlerClass.MessageItemType, handlerClass.Process);
            tasks.Add(receiver);
        }

        public void Start()
        {
            foreach(var t in tasks)
                t.Start();
        }

        public IEnumerable<Exception> Stop()
        {
            var exs = new List<Exception>();
            foreach (var t in tasks)
            { 
                var ex = t.Stop();
                if(ex != null)
                    exs.Add(ex);
            }
            return exs;
        }


        private ServiceReceiverTask CreateReceiverTask(Type type)
        {
            var x = CreateReciever(type);
            return new ServiceReceiverTask(x);
        }

        private ServiceReceiverTask CreateReceiverTask<T>()
        {
            return CreateReceiverTask(typeof (T));
        }

        private TypedMessageReceiver CreateReciever(Type type)
        {
            var config = registry.Get(type);
            var receiver = router.Receiver(config.ServiceName);

            var serviceReceiver = new TypedMessageReceiver(receiver, registry, serializerFactory);

            serviceReceiver.Configure(type);

            return serviceReceiver;
        }

        

        public void Dispose()
        {
            if (router != null)
                router.Dispose();

            foreach(var t in tasks)
                t.Dispose();
        }
    }
}
