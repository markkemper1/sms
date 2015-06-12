using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace Sms.Router
{
	public class RdsBasedConfiguration : IRouterConfiguration
	{
		private readonly Func<IDbConnection> getConnection;
		private readonly string endPointTableName;
		private readonly string endPointMessageTypeColName;
		private readonly string endPointProviderNameColName;
		private readonly string endPointQueueIdentifierColName;
		private readonly string routingTableName;
		private readonly string routingFromColName;
		private readonly string routingToColName;
		private readonly static object LockMe = new object();
		private Dictionary<string, ServiceEndpoint> services;
		private Dictionary<string, List<string>> serviceMapping;
		private DateTime lastQueryTimeUTc = DateTime.MinValue;

		public RdsBasedConfiguration(Func<IDbConnection> getConnection,
			string endPointTableName = "sms_router_endpoint",
			string endPointMessageTypeColName = "MessageType",
			string endPointProviderNameColName = "ProviderName",
			string endPointQueueIdentifierColName = "QueueIdentifier",
			string routingTableName = "sms_router_mapping",
			string routingFromColName = "sms_router_from",
			string routingToColName = "sms_router_to"
			)
		{
			this.getConnection = getConnection;
			this.endPointTableName = endPointTableName;
			this.endPointMessageTypeColName = endPointMessageTypeColName;
			this.endPointProviderNameColName = endPointProviderNameColName;
			this.endPointQueueIdentifierColName = endPointQueueIdentifierColName;
			this.routingTableName = routingTableName;
			this.routingFromColName = routingFromColName;
			this.routingToColName = routingToColName;
		}

		public IEnumerable<ServiceEndpoint> Get(string serviceName)
		{
			lock (LockMe)
			{
				if (DateTime.UtcNow.Subtract(lastQueryTimeUTc).TotalMinutes > 1)
				{
					if (DateTime.UtcNow.Subtract(lastQueryTimeUTc).TotalMinutes > 1)
					{
						LoadServices();
					}
				}

				if (services == null)
					throw new InvalidOperationException("Service configuration not set, service name: " + serviceName);

				List<string> mappings;
				if (serviceMapping.TryGetValue(serviceName, out mappings))
				{
					if (mappings.All(x => services.ContainsKey(x)))
					{
						foreach (var item in mappings)
						{
							yield return services[item];
						}
					}
					else
					{
						yield break;
					}
				}

				if (!services.ContainsKey(serviceName))
					yield break;

				yield return services[serviceName];
			}
		}

		public void Add(ServiceEndpoint service)
		{
			using (var connection = getConnection())
			{
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = String.Format("UPDATE {0} SET {2} = @ProviderName, {3}=@QueueIdentifier WHERE {1} = @MessageType",
						endPointTableName, endPointMessageTypeColName, endPointProviderNameColName, endPointQueueIdentifierColName);

					AddParameters(command, service);

					var affectsCount = command.ExecuteNonQuery();
					if (affectsCount > 0)
					{
						return;
					}

					command.CommandText = String.Format("INSERT INTO {0} ( {1}, {2}, {3} ) VALUES (  @MessageType, @ProviderName,  @QueueIdentifier )",
						endPointTableName, endPointMessageTypeColName, endPointProviderNameColName, endPointQueueIdentifierColName);
					command.ExecuteNonQuery();
				}
			}
			this.lastQueryTimeUTc = DateTime.MinValue;
		}

		public void Remove(ServiceEndpoint service)
		{
			using (var connection = getConnection())
			{
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = String.Format("DELETE FROM {0} WHERE {1} = @MessageType",
						endPointTableName, endPointMessageTypeColName);

					AddParameters(command, service);
					command.ExecuteNonQuery();
				}
			}
			this.lastQueryTimeUTc = DateTime.MinValue;
		}

		public void AddMapping(string fromMessageType, string toMessageType)
		{
			using (var connection = getConnection())
			{
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = String.Format("SELECT 1 FROM {0} WHERE {1} = @From AND {2}=@To",
						routingTableName, routingFromColName, routingToColName);

					AddMappingParameters(command, fromMessageType, toMessageType);

					var exists = command.ExecuteScalar();
					if (exists != null && !DBNull.Value.Equals(exists))
					{
						return;
					}

					command.CommandText = String.Format("INSERT INTO {0} ( {1}, {2} ) VALUES (@From, @To)",
						routingTableName, routingFromColName, routingToColName);
					command.ExecuteNonQuery();
				}
			}
			this.lastQueryTimeUTc = DateTime.MinValue;
		}

		public void RemoveMapping(string fromMessageType, string toMessageType)
		{
			using (var connection = getConnection())
			{
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = String.Format("DELETE FROM {0} WHERE {1} = @From AND {2}=@To",
						routingTableName, routingFromColName, routingToColName);

					AddMappingParameters(command, fromMessageType, toMessageType);
					command.ExecuteNonQuery();
				}
			}
			this.lastQueryTimeUTc = DateTime.MinValue;
		}

		public void Clear()
		{
			using (var connection = getConnection())
			{
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = String.Format("DELETE FROM {0}", routingTableName);
					command.ExecuteNonQuery();

					command.CommandText = String.Format("DELETE FROM {0}",endPointTableName);
					
					command.ExecuteNonQuery();
				}
			}
			this.lastQueryTimeUTc = DateTime.MinValue;
		}

		public void EnsureTableExists()
		{
			var table1 = String.Format(@"IF NOT (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
				WHERE
                 TABLE_NAME = '{0}'))
			BEGIN
				Create Table {0} 
				(
					{1} varchar(255) not null,
					{2} varchar(50) not null,
					{3} varchar(500) not null,
					primary key ({1})
				)
			END", endPointTableName, endPointMessageTypeColName, endPointProviderNameColName, endPointQueueIdentifierColName);

			var table2 = String.Format(@"IF NOT (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
				WHERE
                 TABLE_NAME = '{0}'))
			BEGIN
				Create Table {0} 
				(
					{1} varchar(255) not null,
					{2} varchar(255) not null,
					primary key ({1}, {2})
				)
			END", routingTableName, routingFromColName, routingToColName);

			using (var connection = getConnection())
			{
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = table1;
					Trace.WriteLine(table1);
					command.ExecuteNonQuery();

					command.CommandText = table2;
					Trace.WriteLine(table2);
					command.ExecuteNonQuery();
				}
			}
		}

		private void AddParameters(IDbCommand command, ServiceEndpoint service)
		{
			var p1 = command.CreateParameter();
			p1.ParameterName = "MessageType";
			p1.Value = service.MessageType;
			command.Parameters.Add(p1);

			var p2 = command.CreateParameter();
			p2.ParameterName = "ProviderName";
			p2.Value = service.ProviderName;
			command.Parameters.Add(p2);

			var p3 = command.CreateParameter();
			p3.ParameterName = "QueueIdentifier";
			p3.Value = service.QueueIdentifier;
			command.Parameters.Add(p3);
		}

		private void AddMappingParameters(IDbCommand command, string from, string to)
		{
			var p1 = command.CreateParameter();
			p1.ParameterName = "From";
			p1.Value = from;
			command.Parameters.Add(p1);

			var p2 = command.CreateParameter();
			p2.ParameterName = "To";
			p2.Value = to;
			command.Parameters.Add(p2);
		}

		private void LoadServices()
		{
			using (var connection = getConnection())
			{
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = String.Format("SELECT {1}, {2}, {3} FROM {0} ", endPointTableName, endPointMessageTypeColName, endPointProviderNameColName, endPointQueueIdentifierColName);
					var serviceList = new List<ServiceEndpoint>();
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var service = new ServiceEndpoint()
							{
								MessageType = reader.GetString(0),
								ProviderName = reader.GetString(1),
								QueueIdentifier = reader.GetString(2),
							};
							serviceList.Add(service);
						}
					}
					services = serviceList.ToDictionary(x => x.MessageType, x => x, StringComparer.InvariantCultureIgnoreCase);

					command.CommandText = String.Format("SELECT {1}, {2} FROM {0} ", routingTableName, routingFromColName, routingToColName);
					var mappings = new Dictionary<string, List<string>>();
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							string key = reader.GetString(0);
							if (!mappings.ContainsKey(key)) mappings[key] = new List<string>();
							mappings[key].Add(reader.GetString(1));
						}
					}
					serviceMapping = mappings;
				}
			}
		}
	}
}