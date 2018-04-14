using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConnectionFactory : IMqttConnectionFactory
    {
        private readonly ILogger _logger;
        private readonly IMqttClientFactory _mqttFactory;
        private readonly INameResolver _nameResolver;

        private ConcurrentDictionary<string, MqttConnection> mqttConnections = new ConcurrentDictionary<string, MqttConnection>();

        public MqttConnectionFactory(ILogger logger, IMqttClientFactory mqttFactory, INameResolver nameResolver)
        {
            _logger = logger;
            _mqttFactory = mqttFactory;
            _nameResolver = nameResolver;
        }

        public MqttConnection GetMqttConnection(IRquireMqttConnection attribute)
        {
            var attributeToConfigConverter = new AttributeToConfigConverter(attribute, _nameResolver, _logger);
            var mqttConfiguration = attributeToConfigConverter.GetMqttConfiguration();
            var connection = mqttConnections.GetOrAdd(mqttConfiguration.Name, (c) => new MqttConnection(_mqttFactory, mqttConfiguration, _logger));
            return connection;
        }

        internal bool AllConnectionsConnected()
        {
            foreach (var connection in mqttConnections)
            {
                if (!connection.Value.Connected)
                {
                    return false;
                }
            }
            return true;
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
