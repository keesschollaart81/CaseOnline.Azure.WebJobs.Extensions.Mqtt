using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    public class MqttTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;
        private readonly TraceWriter _traceWriter;

        public MqttTriggerAttributeBindingProvider(INameResolver nameResolver, ILogger logger, TraceWriter traceWriter)
        {
            _nameResolver = nameResolver;
            _logger = logger;
            _traceWriter = traceWriter;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;
            MqttTriggerAttribute mqttTriggerAttribute = parameter.GetCustomAttribute<MqttTriggerAttribute>(inherit: false);

            if (mqttTriggerAttribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            if (parameter.ParameterType != typeof(PublishedMqttMessage))
            {
                throw new InvalidOperationException(string.Format("Can't bind MqttTriggerAttribute to type '{0}'.", parameter.ParameterType));
            }

            _traceWriter.Verbose($"Creating binding for parameter '{parameter.Name}', using custom config creator is '{mqttTriggerAttribute.UseCustomConfigCreator}'");

            MqttConfiguration mqttConfiguration;
            if (!mqttTriggerAttribute.UseCustomConfigCreator)
            {
                var port = _nameResolver.Resolve(mqttTriggerAttribute.PortName ?? "MqttPort");
                int portInt = (port != null) ? int.Parse(port) : 1883;

                var clientId = _nameResolver.Resolve(mqttTriggerAttribute.ClientIdName ?? "MqttClientId");
                if (string.IsNullOrEmpty(clientId))
                {
                    clientId = Guid.NewGuid().ToString();
                }

                var server = _nameResolver.Resolve(mqttTriggerAttribute.ServerName ?? "MqttServer");
                Uri serverUrl;
                try
                {
                    serverUrl = new Uri(server);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not parse ServerUri {server} to a Uri", ex);
                }

                var username = _nameResolver.Resolve(mqttTriggerAttribute.UsernameName ?? "MqttUsername");
                var password = _nameResolver.Resolve(mqttTriggerAttribute.PasswordName ?? "MqttPassword");

                var options = new ManagedMqttClientOptionsBuilder()
                   .WithAutoReconnectDelay(mqttTriggerAttribute.ReconnectDelay)
                   .WithClientOptions(new MqttClientOptionsBuilder()
                       .WithClientId(clientId)
                       .WithTcpServer(server, portInt)
                       .WithCredentials(username, password)
                       .Build())
                   .Build();

                var topics = mqttTriggerAttribute.Topics.Select(t => new TopicFilter(t, MqttQualityOfServiceLevel.AtLeastOnce)).ToArray();

                mqttConfiguration = new MqttConfiguration(serverUrl, options, topics);
            }
            else
            {
                ICreateMqttConfig customConfigCreator;
                MqttConfig customConfig;
                try
                {
                    customConfigCreator = (ICreateMqttConfig)Activator.CreateInstance(mqttTriggerAttribute.MqttConfigCreatorType);
                }
                catch (Exception ex)
                {
                    _traceWriter.Error($"Enexpected exception while instantiating custom config creator of type {mqttTriggerAttribute.MqttConfigCreatorType.FullName}", ex);
                    throw;
                }

                try
                {
                    customConfig = customConfigCreator.Create(_nameResolver, _logger);
                }
                catch (Exception ex)
                {
                    _traceWriter.Error($"Enexpected exception while getting creating a config via type {mqttTriggerAttribute.MqttConfigCreatorType.FullName}", ex);
                    throw;
                }

                mqttConfiguration = new MqttConfiguration(customConfig.ServerUrl, customConfig.Options, customConfig.Topics);
            }

            var client = new MqttFactory();

            _traceWriter.Verbose($"Succesfully created binding for parameter '{parameter.Name}'");

            return Task.FromResult<ITriggerBinding>(new MqttTriggerBinding(parameter, client, mqttConfiguration, _logger, _traceWriter));
        }
    }
}
