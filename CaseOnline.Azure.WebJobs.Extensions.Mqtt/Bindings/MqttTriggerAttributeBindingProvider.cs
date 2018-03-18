using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    public class MqttTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly TraceWriter _logger;

        public MqttTriggerAttributeBindingProvider(INameResolver nameResolver, TraceWriter logger)
        {
            _nameResolver = nameResolver;
            _logger = logger;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            _logger.Info("MqttTriggerAttributeBindingProvider.TryCreateAsync");

            if (context == null)
            {
                throw new ArgumentNullException("context");
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

            IManagedMqttClientOptions options;
            if (mqttTriggerAttribute.UseManagedMqttClient)
            {
                var port = _nameResolver.Resolve(mqttTriggerAttribute.PortName ?? "MqttPort");
                int portInt = (port != null) ? int.Parse(port) : 1883;

                var clientId = _nameResolver.Resolve(mqttTriggerAttribute.ClientIdName ?? "ClientId");
                if (string.IsNullOrEmpty(clientId)) clientId = Guid.NewGuid().ToString();

                var server = _nameResolver.Resolve(mqttTriggerAttribute.ServerName ?? "MqttServer");
                var username = _nameResolver.Resolve(mqttTriggerAttribute.UsernameName ?? "MqttUsername");
                var password = _nameResolver.Resolve(mqttTriggerAttribute.PasswordName ?? "MqttPassword");

                options = new ManagedMqttClientOptionsBuilder()
                   .WithAutoReconnectDelay(mqttTriggerAttribute.ReconnectDelay)
                   .WithClientOptions(new MqttClientOptionsBuilder()
                       .WithClientId(clientId)
                       .WithTcpServer(server, portInt)
                       .WithCredentials(username, password)
                       .Build())
                   .Build();
            }
            else
            {
                options = mqttTriggerAttribute.ManagedMqttClientOptions;
            }

            var config = new MqttConfiguration(options, mqttTriggerAttribute.Topics);
            var clientFactory = new MqttFactory();

            return Task.FromResult<ITriggerBinding>(new MqttTriggerBinding(parameter, clientFactory, config, _logger));
        }
    }
}
