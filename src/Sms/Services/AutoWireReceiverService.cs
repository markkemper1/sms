using System;
using System.Collections.Generic;
using System.Linq;

namespace Sms.Services
{

    public abstract class AutoConfigureServiceBase : ReceiverServiceBase
    {
        static readonly Type[] constructorArgs = new Type[0];

        public AutoConfigureServiceBase()
        {
            var allServices = (from x in this.GetType().Assembly.GetTypes()
                                        let y = x.BaseType
                                        where !x.IsAbstract && !x.IsInterface &&
                                        y != null && y.IsGenericType &&
                                        y.GetGenericTypeDefinition() == typeof(ServiceReceiver<>)
                                        select x);

            foreach (var serviceType in allServices)
            {
                var serviceReceiver = (IServiceReceiver) Build(serviceType);
                this.Configure(serviceReceiver);
            }
        }

        public virtual object Build(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            var constructor = type.GetConstructor(constructorArgs);

            if (constructor == null)
            {
                throw new InvalidOperationException("The ServiceReceiver must have a parameter less constructor or override the Build method.");
            }

            return constructor.Invoke(new object[0]);
        }

    }

    public abstract class ReceiverServiceBase : IDisposable
    {
        private List<Exchange> exchanges = new List<Exchange>();
        //private ReceiveTask<HourlyEvent> task;

        public ReceiverServiceBase Configure(IServiceReceiver serviceReceiver)
        {
            var exchange = new Exchange();
            serviceReceiver.Sink = exchange;
            exchange.Register(serviceReceiver.MessageItemType, serviceReceiver.Process);

            exchanges.Add(exchange);
            return this;
        }

        public virtual void Start()
        {
            foreach(var ex in exchanges)
                ex.Start();
        }

        public virtual IList<Exception> Stop()
        {
            var list = new List<Exception>();

            foreach (var exchange in exchanges)
            {
                var exceptions = exchange.Stop();

                foreach (var ex in exceptions)
                {
                    list.Add(ex);
                    Log(ex);
                }

                exchange.Dispose();
            }

            return list;
        }

        public abstract void Log(Exception ex);

        public void Dispose()
        {
            this.Stop();
        }
    }
}
