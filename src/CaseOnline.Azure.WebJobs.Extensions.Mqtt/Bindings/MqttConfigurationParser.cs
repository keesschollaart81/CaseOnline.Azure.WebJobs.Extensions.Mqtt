using System;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Parses a <see cref="MqttTriggerAttribute"/> to a <see cref="MqttConfiguration"/>.
    /// </summary>
    public class MqttConfigurationParser : IMqttConfigurationParser
    {
        private const string DefaultAppsettingsKeyForConnectionString = "MqttConnection";

        private readonly TimeSpan _defaultReconnectTime = TimeSpan.FromSeconds(5);
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttConfigurationParser"/> class.
        /// </summary>
        /// <param name="nameResolver">The name resolver.</param>
        /// <param name="logger">The logger.</param>
        public MqttConfigurationParser(INameResolver nameResolver, ILogger logger)
        { 
            _nameResolver = nameResolver;
            _logger = logger;
        }

        /// <summary>
        /// Gets the MQTT configuration from the given attribute.
        /// </summary>
        /// <param name="mqttAttribute">The attribute to parse from.</param>
        /// <returns>
        /// The configuration.
        /// </returns>
        public MqttConfiguration Parse(MqttBaseAttribute mqttAttribute)
        {
            return mqttAttribute.MqttConfigCreatorType == null
                ? GetConfigurationViaAttributeValues(mqttAttribute)
                : GetConfigurationViaCustomConfigCreator(mqttAttribute);
        }

        private MqttConfiguration GetConfigurationViaAttributeValues(MqttBaseAttribute mqttAttribute)
        {
            var name = mqttAttribute.ConnectionString ?? DefaultAppsettingsKeyForConnectionString;
            var connectionString = _nameResolver.Resolve(mqttAttribute.ConnectionString) ?? _nameResolver.Resolve(DefaultAppsettingsKeyForConnectionString);
            var mqttConnectionString = new MqttConnectionString(connectionString);

            var mqttClientOptions = GetMqttClientOptions(mqttConnectionString);

            var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
               .WithAutoReconnectDelay(_defaultReconnectTime)
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
                // Need to implement TLS verification sometime
                mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithTls(true, false, false);
            }

            return mqttClientOptionsBuilder.Build();
        }

        private MqttConfiguration GetConfigurationViaCustomConfigCreator(MqttBaseAttribute mqttAttribute)
        {
            CustomMqttConfig customConfig;
            try
            {
                var customConfigCreator = (ICreateMqttConfig)Activator.CreateInstance(mqttAttribute.MqttConfigCreatorType);
                customConfig = customConfigCreator.Create(_nameResolver, _logger);
            }
            catch (Exception ex)
            {
                throw new InvalidCustomConfigCreatorException($"Unexpected exception while getting creating a config via type {mqttAttribute.MqttConfigCreatorType.FullName}", ex);
            }

            return new MqttConfiguration(customConfig.Name, customConfig.Options);
        }
    }
}
