using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using MQTTnet.Client.Options;
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
        /// <param name="loggerFactory">The logger factory.</param>
        public MqttConfigurationParser(INameResolver nameResolver, ILoggerFactory loggerFactory)
        {
            _nameResolver = nameResolver;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));
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
            var name = mqttAttribute.ConnectionString;
            var connectionString = _nameResolver.Resolve(mqttAttribute.ConnectionString);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _nameResolver.Resolve(DefaultAppsettingsKeyForConnectionString);
                name = DefaultAppsettingsKeyForConnectionString;
            }
            var mqttConnectionString = new MqttConnectionString(connectionString, name);

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
                var certificates = new List<byte[]>();
                if (connectionString.Certificate != null)
                {
                    var serializedServerCertificate = new X509Certificate(connectionString.Certificate)
                        .Export(X509ContentType.Cert);
                    certificates.Add(serializedServerCertificate);
                }

                mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = true,
                    Certificates = certificates,
#if DEBUG                   
                    CertificateValidationCallback = (X509Certificate x, X509Chain y, SslPolicyErrors z, IMqttClientOptions o) =>
                    {
                        return true;
                    },
#endif
                    AllowUntrustedCertificates = false,
                    IgnoreCertificateChainErrors = false,
                    IgnoreCertificateRevocationErrors = false
                });
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

            string name = customConfig.Name;
            if (mqttAttribute is MqttTriggerAttribute)
            {
                // Make sure that if custom configurations are (re)used over multiple triggers, they all get their own unique name
                // This makes sure that the actual connection is not reused
                name = customConfig.Name + $".{Guid.NewGuid()}";
            }
            return new MqttConfiguration(name, customConfig.Options);
        }
    }
}
