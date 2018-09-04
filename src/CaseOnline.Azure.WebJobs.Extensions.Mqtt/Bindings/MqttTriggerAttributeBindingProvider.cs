using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Provides binding of the <see cref="MqttTriggerAttribute"/>.
    /// </summary>
    public class MqttTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly IMqttConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttTriggerAttribute"/>.
        /// </summary>
        /// <param name="nameResolver">The name resolver.</param>
        /// <param name="connectionFactory">the connection factory.</param>
        /// <param name="loggerFactory">The loggerFactory.</param>
        internal MqttTriggerAttributeBindingProvider(INameResolver nameResolver, IMqttConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            _nameResolver = nameResolver;
            _connectionFactory = connectionFactory;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var mqttTriggerAttribute = GetMqttTriggerAttribute(context.Parameter);
            if (mqttTriggerAttribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            _logger.LogDebug($"Creating binding for parameter '{context.Parameter.Name}'");

            ITriggerBinding mqttTriggerBinding = GetMqttTriggerBinding(context.Parameter, mqttTriggerAttribute);

            _logger.LogDebug($"Succesfully created binding for parameter '{context.Parameter.Name}'");

            return Task.FromResult(mqttTriggerBinding);
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

        private MqttTriggerBinding GetMqttTriggerBinding(ParameterInfo parameter, MqttTriggerAttribute mqttTriggerAttribute)
        {
            TopicFilter[] topics;
            var mqttConnection = _connectionFactory.GetMqttConnection(mqttTriggerAttribute);
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
