using System;
using System.Collections.Generic;
using System.Linq;
using Sms.Internals;

namespace Sms.Services
{
    public class SerializerFactory : ISerializerFactory
    {
        private Dictionary<string, ISerializer> serializers;

        static readonly object LockMe = new object();

        public ISerializer Get(string provider)
        {
            return GetSeralizer(provider);
        }

        private ISerializer GetSeralizer(string provider)
        {
            if (serializers == null) CreateSerializers();

            if (!serializers.ContainsKey(provider))
                throw new ArgumentException("The serializer is not supported: " + provider);

            return serializers[provider];
        }

        private void CreateSerializers()
        {
            if (serializers != null) return;

            lock (LockMe)
            {
                if (serializers != null) return;
                serializers = GenericFactory.FindAndBuild<ISerializer>().ToDictionary(x => x.Name, x => x);
            }
        }


    }
}
