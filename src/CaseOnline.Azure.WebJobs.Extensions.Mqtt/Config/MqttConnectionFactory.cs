using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConnectionFactory : IMqttConnectionFactory
    {
        private readonly ILogger _logger;
        private readonly IManagedMqttClientFactory _mqttFactory;
        private readonly INameResolver _nameResolver;

        private readonly ConcurrentDictionary<string, MqttConnection> _mqttConnections = new ConcurrentDictionary<string, MqttConnection>();

        public MqttConnectionFactory(ILoggerFactory loggerFactory, IManagedMqttClientFactory mqttFactory, INameResolver nameResolver)
        {
            _mqttFactory = mqttFactory;
            _nameResolver = nameResolver;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));
        }

        public MqttConnectionFactory()
        {
        }

        public MqttConnection GetMqttConnection(IRquireMqttConnection attribute)
        {
            var attributeToConfigConverter = new AttributeToConfigConverter(attribute, _nameResolver, _logger);
            var mqttConfiguration = attributeToConfigConverter.GetMqttConfiguration();
            if (_mqttConnections.ContainsKey(mqttConfiguration.Name) && attribute is MqttTriggerAttribute)
            {
                throw new InvalidOperationException($"Error setting up listener for this attribute. Connectionstring '{mqttConfiguration.Name}' is already used by another Trigger. Connections can only be reused for output bindings. Each trigger needs it own connectionstring");
            }
            var connection = _mqttConnections.GetOrAdd(mqttConfiguration.Name, (c) => new MqttConnection(_mqttFactory, mqttConfiguration, _logger));
            return connection;
        }

        internal bool AllConnectionsConnected()
        {
            return _mqttConnections.All(x => x.Value.ConnectionState == ConnectionState.Connected);
        }

        public async Task DisconnectAll()
        {
            foreach (var connection in _mqttConnections)
            {
                await connection.Value.StopAsync().ConfigureAwait(false);
                connection.Value.Dispose();
            }
        }
    }
}
