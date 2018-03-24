using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

            _traceWriter.Info($"Creating binding for parameter '{parameter.Name}', using custom config creator is '{mqttTriggerAttribute.UseCustomConfigCreator}'");
            

            IManagedMqttClientOptions options;
            TopicFilter[] topics;
            if (!mqttTriggerAttribute.UseCustomConfigCreator)
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

                topics = mqttTriggerAttribute.Topics.Select(t => new TopicFilter(t, MqttQualityOfServiceLevel.AtLeastOnce)).ToArray();
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
                options = customConfig.Options;
                topics = customConfig.Topics;
            }

            var config = new MqttConfiguration(options, topics);

            var client = new MqttFactory();

            _traceWriter.Info($"Succesfully created binding for parameter '{parameter.Name}'");

            return Task.FromResult<ITriggerBinding>(new MqttTriggerBinding(parameter, client, config, _logger, _traceWriter));
        }
    }
}
