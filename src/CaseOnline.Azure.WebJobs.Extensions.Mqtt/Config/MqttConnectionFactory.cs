using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConnectionFactory : IMqttConnectionFactory
    {
        private readonly ILogger _logger;
        private readonly IManagedMqttClientFactory _mqttFactory;
        private readonly INameResolver _nameResolver;

        private ConcurrentDictionary<string, MqttConnection> mqttConnections = new ConcurrentDictionary<string, MqttConnection>();

        public MqttConnectionFactory(ILogger logger, IManagedMqttClientFactory mqttFactory, INameResolver nameResolver)
        {
            _logger = logger;
            _mqttFactory = mqttFactory;
            _nameResolver = nameResolver;
        }

        public MqttConnection GetMqttConnection(IRquireMqttConnection attribute)
        {
            var attributeToConfigConverter = new AttributeToConfigConverter(attribute, _nameResolver, _logger);
            var mqttConfiguration = attributeToConfigConverter.GetMqttConfiguration();
            if (mqttConnections.ContainsKey(mqttConfiguration.Name) && attribute is MqttTriggerAttribute)
            {
                throw new Exception($"Error setting up listener for this attribute. Connectionstring '{mqttConfiguration.Name}' is already used by another Trigger. Connections can only be reused for output bindings. Each trigger needs it own connectionstring");
            }
            var connection = mqttConnections.GetOrAdd(mqttConfiguration.Name, (c) => new MqttConnection(_mqttFactory, mqttConfiguration, _logger));
            return connection;
        }

        internal bool AllConnectionsConnected()
        {
            return mqttConnections.All(x => x.Value.ConnectionState == ConnectionState.Connected);
        }

        public async Task DisconnectAll()
        {
            foreach (var connection in mqttConnections)
            {
                await connection.Value.StopAsync().ConfigureAwait(false);
                connection.Value.Dispose();
            }
        }
    }
}
