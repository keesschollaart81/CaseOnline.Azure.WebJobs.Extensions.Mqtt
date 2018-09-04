using System;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Converts a <see cref="MqttTriggerAttribute"/> to a <see cref="MqttConfiguration"/>.
    /// </summary>
    public class AttributeToConfigConverter
    {
        private const string DefaultAppsettingsKeyForConnectionString = "MqttConnection";

        private readonly TimeSpan _detaultReconnectTime = TimeSpan.FromSeconds(5);
        private readonly IRquireMqttConnection _mqttTriggerAttribute;
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeToConfigConverter"/> class.
        /// </summary>
        /// <param name="source">The trigger attribute.</param>
        /// <param name="nameResolver">The name resolver.</param>
        /// <param name="logger">The logger.</param>
        public AttributeToConfigConverter(IRquireMqttConnection source, INameResolver nameResolver, ILogger logger)
        {
            _mqttTriggerAttribute = source;
            _nameResolver = nameResolver;
            _logger = logger;
        }

        /// <summary>
        /// Gets the MQTT configuration from this attribute.
        /// </summary>
        /// <returns>
        /// The configuration.
        /// </returns>
        public MqttConfiguration GetMqttConfiguration()
        {
            return _mqttTriggerAttribute.MqttConfigCreatorType == null
                ? GetConfigurationViaAttributeValues()
                : GetConfigurationViaCustomConfigCreator();
        }

        private MqttConfiguration GetConfigurationViaAttributeValues()
        {
            var name = _mqttTriggerAttribute.ConnectionString ?? DefaultAppsettingsKeyForConnectionString;
            var connectionString = _nameResolver.Resolve(_mqttTriggerAttribute.ConnectionString) ?? _nameResolver.Resolve(DefaultAppsettingsKeyForConnectionString);
            var mqttConnectionString = new MqttConnectionString(connectionString);

            var mqttClientOptions = GetMqttClientOptions(mqttConnectionString);

            var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
               .WithAutoReconnectDelay(_detaultReconnectTime)
               .WithClientOptions(mqttClientOptions)
               .Build();

            return new MqttConfiguration(name, managedMqttClientOptions);
        }

        private IMqttClientOptions GetMqttClientOptions(MqttConnectionString connectionString)
        {
            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                           .WithClientId(connectionString.ClientId)
                           .WithTcpServer(connectionString.Server, connectionString.Port);

            if (!string.IsNullOrEmpty(connectionString.Username) || !string.IsNullOrEmpty(connectionString.Password))
            {
                _logger.LogTrace($"Using authentication, username: '{connectionString.Username}'");
                mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithCredentials(connectionString.Username, connectionString.Password);
            }

            if (connectionString.Tls)
            {
                //todo TLS verification
                mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithTls(true, false, false);
            }

            return mqttClientOptionsBuilder.Build();
        }

        private MqttConfiguration GetConfigurationViaCustomConfigCreator()
        {
            CustomMqttConfig customConfig;
            try
            {
                var customConfigCreator = (ICreateMqttConfig)Activator.CreateInstance(_mqttTriggerAttribute.MqttConfigCreatorType);
                customConfig = customConfigCreator.Create(_nameResolver, _logger);
            }
            catch (Exception ex)
            {
                throw new InvalidCustomConfigCreatorException($"Unexpected exception while getting creating a config via type {_mqttTriggerAttribute.MqttConfigCreatorType.FullName}", ex);
            }

            return new MqttConfiguration(customConfig.Name, customConfig.Options);
        }
    }
}
