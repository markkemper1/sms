using System;
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
                                        y.GetGenericTypeDefinition() == typeof(ServiceReciever<>)
                                        select x);

            foreach (var serviceType in allServices)
            {
                var serviceReciever = (IServiceReciever) Build(serviceType);
                this.Configure(serviceReciever);
            }
        }

        public virtual object Build(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            var constructor = type.GetConstructor(constructorArgs);

            if (constructor == null)
            {
                throw new InvalidOperationException("The ServiceReciever must have a parameter less constructor or override the Build method.");
            }

            return constructor.Invoke(new object[0]);
        }

    }

    public abstract class ReceiverServiceBase
    {
        private Exchange exchange;
        //private RecieveTask<HourlyEvent> task;

        public ReceiverServiceBase()
        {
            exchange = new Exchange();
        }

        public ReceiverServiceBase Configure(IServiceReciever serviceReciever)
        {
            serviceReciever.Sink = exchange;
            exchange.Register(serviceReciever.MessageItemType, serviceReciever.Process);
            return this;
        }

        public void Start()
        {
            exchange.Start();
        }

        public void Stop()
        {
            if (exchange != null)
            {
                var exceptions = exchange.Stop();

                foreach (var ex in exceptions)
                    Log(ex);

                exchange.Dispose();
            }
        }

        public virtual void Log(Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex.ToString());
        }
    }
}
