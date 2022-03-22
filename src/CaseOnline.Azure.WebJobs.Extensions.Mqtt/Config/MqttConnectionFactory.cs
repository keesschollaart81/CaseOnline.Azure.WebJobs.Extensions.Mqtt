using System.Collections.Concurrent;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;

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
        _logger = loggerFactory?.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt")) ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public MqttConnection GetMqttConnection(MqttBaseAttribute attribute)
    {
        var mqttConfiguration = _mqttConfigurationParser.Parse(attribute);
        if (_mqttConnections.ContainsKey(mqttConfiguration.Name) && attribute is MqttTriggerAttribute && _mqttConnections[mqttConfiguration.Name].UsedByTrigger)
        {
            throw new InvalidOperationException($"Error setting up listener for this attribute. Connectionstring '{mqttConfiguration.Name}' is already used by another Trigger. Connections can only be reused for output bindings. Each trigger needs it own connectionstring");
        }

        var connection = _mqttConnections.GetOrAdd(mqttConfiguration.Name, _ => new MqttConnectionEntry(new MqttConnection(_mqttFactory, mqttConfiguration, _logger)));

        if (attribute is MqttTriggerAttribute)
        {
            _mqttConnections[mqttConfiguration.Name].UsedByTrigger = true;
        }
        return connection.MqttConnection;
    }

    internal bool AllConnectionsConnected()
    {
        return _mqttConnections.All(x => x.Value.MqttConnection.ConnectionState == ConnectionState.Connected);
    }

    public async Task DisconnectAll()
    {
        foreach (var mqttConnection in _mqttConnections.Select(connection => connection.Value.MqttConnection))
        {
            await mqttConnection.StopAsync().ConfigureAwait(false);
            mqttConnection.Dispose();
        }
        _mqttConnections.Clear();
    }

    private sealed class MqttConnectionEntry
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
