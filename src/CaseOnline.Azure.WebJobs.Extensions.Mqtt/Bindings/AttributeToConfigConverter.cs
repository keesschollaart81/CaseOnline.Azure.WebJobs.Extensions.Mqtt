using System;
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
    public class AttributeToConfigConverter
    {
        private readonly MqttTriggerAttribute _mqttTriggerAttribute;
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;

        private const int DetaultMqttPort = 1883;
        private TimeSpan DetaultReconnectTime = TimeSpan.FromSeconds(5);
        private const string SettingsKeyForPort = "MqttPort";
        private const string SettingsKeyForClientId = "MqttClientId";
        private const string SettingsKeyForServer = "MqttServer";
        private const string SettingsKeyForUsername = "MqttUsername";
        private const string SettingsKeyForPassword = "MqttPassword";

        public AttributeToConfigConverter(MqttTriggerAttribute source, INameResolver nameResolver, ILogger logger)
        {
            _mqttTriggerAttribute = source;
            _nameResolver = nameResolver;
            _logger = logger;
        }

        public MqttConfiguration GetMqttConfiguration()
        {
            MqttConfiguration mqttConfiguration;

            if (!_mqttTriggerAttribute.UseCustomConfigCreator)
            {
                mqttConfiguration = GetConfigurationViaAttributeValues();
            }
            else
            {
                mqttConfiguration = GetConfigurationViaCustomConfigCreator();
            }
            return mqttConfiguration;
        }

        private MqttConfiguration GetConfigurationViaAttributeValues()
        {
            var port = _nameResolver.Resolve(_mqttTriggerAttribute.PortName ?? SettingsKeyForPort);
            int portInt = (port != null) ? int.Parse(port) : DetaultMqttPort;

            var clientId = _nameResolver.Resolve(_mqttTriggerAttribute.ClientIdName ?? SettingsKeyForClientId);
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = Guid.NewGuid().ToString();
            }

            var server = _nameResolver.Resolve(_mqttTriggerAttribute.ServerName ?? SettingsKeyForServer);      
            if (string.IsNullOrEmpty(server)) throw new Exception("No server hostname configured, please set the server via the MqttTriggerAttribute, using the application settings via the Azure Portal or using the local.settings.json");     

            var username = _nameResolver.Resolve(_mqttTriggerAttribute.UsernameName ?? SettingsKeyForUsername);
            var password = _nameResolver.Resolve(_mqttTriggerAttribute.PasswordName ?? SettingsKeyForPassword);

            var options = new ManagedMqttClientOptionsBuilder()
               .WithAutoReconnectDelay(DetaultReconnectTime)
               .WithClientOptions(new MqttClientOptionsBuilder()
                   .WithClientId(clientId)
                   .WithTcpServer(server, portInt)
                   .WithCredentials(username, password)
                   .Build())
               .Build();

            var topics = _mqttTriggerAttribute.Topics.Select(t => new TopicFilter(t, MqttQualityOfServiceLevel.AtLeastOnce));

            return new MqttConfiguration(options, topics);
        }

        private MqttConfiguration GetConfigurationViaCustomConfigCreator()
        {
            ICreateMqttConfig customConfigCreator;
            MqttConfig customConfig;
            try
            {
                customConfigCreator = (ICreateMqttConfig)Activator.CreateInstance(_mqttTriggerAttribute.MqttConfigCreatorType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Enexpected exception while instantiating custom config creator of type {_mqttTriggerAttribute.MqttConfigCreatorType.FullName}", ex);
            }

            try
            {
                customConfig = customConfigCreator.Create(_nameResolver, _logger);
            }
            catch (Exception ex)
            {
                throw new Exception($"Enexpected exception while getting creating a config via type {_mqttTriggerAttribute.MqttConfigCreatorType.FullName}", ex);
            }

            return new MqttConfiguration(customConfig.Options, customConfig.Topics);
        }
    }
}
