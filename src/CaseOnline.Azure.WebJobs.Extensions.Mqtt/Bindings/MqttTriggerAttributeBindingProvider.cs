using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Provides binding of the <see cref="MqttTriggerAttribute"/>.
    /// </summary>
    public class MqttTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly MqttConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttTriggerAttribute"/>.
        /// </summary>
        /// <param name="nameResolver">The name resolver</param>
        /// <param name="connectionFactory">the connection factory</param>
        /// <param name="logger">The logger</param>
        internal MqttTriggerAttributeBindingProvider(INameResolver nameResolver, MqttConnectionFactory connectionFactory, ILogger logger)
        {
            _nameResolver = nameResolver;
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var mqttTriggerAttribute = GetMqttTriggerAttribute(context.Parameter);
            if (mqttTriggerAttribute == null)
            {
                return null;
            }

            _logger.LogDebug($"Creating binding for parameter '{context.Parameter.Name}'");

            var mqttTriggerBinding = await GetMqttTriggerBindingAsync(context.Parameter, mqttTriggerAttribute).ConfigureAwait(false);

            _logger.LogDebug($"Succesfully created binding for parameter '{context.Parameter.Name}'");

            return mqttTriggerBinding;
        }

        private static MqttTriggerAttribute GetMqttTriggerAttribute(ParameterInfo parameter)
        {
            var mqttTriggerAttribute = parameter.GetCustomAttribute<MqttTriggerAttribute>(inherit: false);

            if (mqttTriggerAttribute == null)
            {
                return null;
            }

            if (parameter.ParameterType != typeof(IMqttMessage))
            {
                throw new InvalidOperationException($"Can't bind MqttTriggerAttribute to type '{parameter.ParameterType}'.");
            }

            return mqttTriggerAttribute;
        }

        private async Task<MqttTriggerBinding> GetMqttTriggerBindingAsync(ParameterInfo parameter, MqttTriggerAttribute mqttTriggerAttribute)
        {
            TopicFilter[] topics;
            var mqttConnection = await _connectionFactory.GetMqttConnectionAsync(mqttTriggerAttribute).ConfigureAwait(false);
            try
            {
                topics = mqttTriggerAttribute.Topics.Select(t => new TopicFilter(t, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }

            return new MqttTriggerBinding(parameter, mqttConnection, topics, _logger);
        }
    }
}
