using System;
using System.Data.Common;
using System.Linq;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Converts a <see cref="MqttTriggerAttribute"/> to a <see cref="MqttConfiguration"/>.
    /// </summary>
    public class AttributeToConfigConverter
    {
        private const int DetaultMqttPort = 1883;
        private const string ConnectionStringForPort = "Port";
        private const string ConnectionStringForClientId = "ClientId";
        private const string ConnectionStringForServer = "Server";
        private const string ConnectionStringForUsername = "Username";
        private const string ConnectionStringForPassword = "Password";

        private readonly TimeSpan _detaultReconnectTime = TimeSpan.FromSeconds(5);
        private readonly MqttTriggerAttribute _mqttTriggerAttribute;
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;
        private readonly DbConnectionStringBuilder _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeToConfigConverter"/> class.
        /// </summary>
        /// <param name="source">The trigger attribute.</param>
        /// <param name="nameResolver">The name resolver.</param>
        /// <param name="logger">The logger.</param>
        public AttributeToConfigConverter(MqttTriggerAttribute source, INameResolver nameResolver, ILogger logger)
        {
            _connectionString = new DbConnectionStringBuilder()
            {
                ConnectionString = nameResolver.Resolve(source.ConnectionString)
            };
        
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
            return !_mqttTriggerAttribute.UseCustomConfigCreator 
                ? GetConfigurationViaAttributeValues() 
                : GetConfigurationViaCustomConfigCreator();
        }

        private MqttConfiguration GetConfigurationViaAttributeValues()
        {

            var port = _connectionString.TryGetValue(ConnectionStringForPort, out var portAsString) &&
                int.TryParse(portAsString as string, out var portAsInt)
                    ? portAsInt
                    : DetaultMqttPort;

            var clientId = _connectionString.TryGetValue(ConnectionStringForClientId, out var clientIdValue) && !string.IsNullOrEmpty(clientIdValue as string)
                ? clientIdValue.ToString()
                : Guid.NewGuid().ToString();

            var server = _connectionString.TryGetValue(ConnectionStringForServer, out var serverValue)
                ? serverValue.ToString()
                : throw new Exception("No server hostname configured, please set the server via the MqttTriggerAttribute, using the application settings via the Azure Portal or using the local.settings.json"); ;

            var username = _connectionString.TryGetValue(ConnectionStringForUsername, out var userNameValue)
                ? userNameValue.ToString()
                : null;

            var password = _connectionString.TryGetValue(ConnectionStringForPassword, out var passwordValue)
                ? passwordValue.ToString()
                : null;

            var options = new ManagedMqttClientOptionsBuilder()
               .WithAutoReconnectDelay(_detaultReconnectTime)
               .WithClientOptions(new MqttClientOptionsBuilder()
                   .WithClientId(clientId)
                   .WithTcpServer(server, port)
                   .WithCredentials(username, password)
                   .Build())
               .Build();

            var topics = _mqttTriggerAttribute.Topics.Select(t => new TopicFilter(t, MqttQualityOfServiceLevel.AtLeastOnce));

            return new MqttConfiguration(options, topics);
        }

        private MqttConfiguration GetConfigurationViaCustomConfigCreator()
        {
            MqttConfig customConfig;
            try
            {
                var customConfigCreator = (ICreateMqttConfig)Activator.CreateInstance(_mqttTriggerAttribute.MqttConfigCreatorType);
                customConfig = customConfigCreator.Create(_nameResolver, _logger);
            }
            catch (Exception ex)
            {
                throw new InvalidCustomConfigCreatorException($"Unexpected exception while getting creating a config via type {_mqttTriggerAttribute.MqttConfigCreatorType.FullName}", ex);
            }

            return new MqttConfiguration(customConfig.Options, customConfig.Topics);
        }
    }
}
