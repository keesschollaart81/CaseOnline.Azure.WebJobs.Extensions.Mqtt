using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConnectionFactory : IMqttConnectionFactory
    {
        private readonly ILogger _logger;
        private readonly IManagedMqttClientFactory _mqttFactory;
        private readonly IMqttConfigurationParser _mqttConfigurationParser;

        private readonly ConcurrentDictionary<string, MqttConnectionEntry> _mqttConnections = new ConcurrentDictionary<string, MqttConnectionEntry>();

        public MqttConnectionFactory(ILoggerFactory loggerFactory, IManagedMqttClientFactory mqttFactory, IMqttConfigurationParser mqttConfigurationParser)
        {
            _mqttFactory = mqttFactory;
            _mqttConfigurationParser = mqttConfigurationParser;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));
        }

        public MqttConnection GetMqttConnection(MqttBaseAttribute attribute)
        {
            var mqttConfiguration = _mqttConfigurationParser.Parse(attribute);
            if (_mqttConnections.ContainsKey(mqttConfiguration.Name) && attribute is MqttTriggerAttribute)
            {
                if (_mqttConnections[mqttConfiguration.Name].UsedByTrigger)
                {
                    throw new InvalidOperationException($"Error setting up listener for this attribute. Connectionstring '{mqttConfiguration.Name}' is already used by another Trigger. Connections can only be reused for output bindings. Each trigger needs it own connectionstring");
                }
                else
                {
                    _mqttConnections[mqttConfiguration.Name].UsedByTrigger = true;
                }
            }
            var connection = _mqttConnections.GetOrAdd(mqttConfiguration.Name, (c) => new MqttConnectionEntry(new MqttConnection(_mqttFactory, mqttConfiguration, _logger)));
            return connection.MqttConnection;
        }

        internal bool AllConnectionsConnected()
        {
            return _mqttConnections.All(x => x.Value.MqttConnection.ConnectionState == ConnectionState.Connected);
        }

        public async Task DisconnectAll()
        {
            foreach (var connection in _mqttConnections)
            {
                await connection.Value.MqttConnection.StopAsync().ConfigureAwait(false);
                connection.Value.MqttConnection.Dispose();
            }
            _mqttConnections.Clear();
        }

        private class MqttConnectionEntry
        {
            public MqttConnectionEntry(MqttConnection mqttConnection)
            {
                MqttConnection = mqttConnection;
                UsedByTrigger = false;
            }

            public MqttConnection MqttConnection { get; set; }

            public bool UsedByTrigger { get; set; }
        }
    }
}
