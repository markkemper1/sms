using System;
using System.Collections.Generic;
using System.Linq;
using Sms.Internals;

namespace Sms.Services
{
	public class SerializerFactory : ISerializerFactory
	{
		private readonly Dictionary<string, ISerializer> serializers;

		private static readonly object LockMe = new object();

		private SerializerFactory()
		{
			serializers = new Dictionary<string, ISerializer>();
		}

		public SerializerFactory(IEnumerable<ISerializer> serializers)
		{
			if (serializers == null) throw new ArgumentNullException("serializers");

			this.serializers = serializers.ToDictionary(x => x.Name, x => x);
		}

		public static SerializerFactory CreateEmpty()
		{
			return new SerializerFactory();
		}

		public void Register(ISerializer serializer)
		{
			if (serializer == null) throw new ArgumentNullException("serializer");
			serializers.Add(serializer.Name, serializer);
		}

		public ISerializer Get(string provider)
		{
			return GetSeralizer(provider);
		}

		private ISerializer GetSeralizer(string provider)
		{
			if (serializers == null)
				throw new InvalidOperationException("No serializers have been configured.: " + provider); ;

			if (!serializers.ContainsKey(provider))
				throw new ArgumentException("The serializer is not supported: " + provider);

			return serializers[provider];
		}



	}
}
