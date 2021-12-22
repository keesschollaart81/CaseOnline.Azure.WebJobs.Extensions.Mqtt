using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Provides binding of the <see cref="MqttTriggerAttribute"/>.
    /// </summary>
    public class MqttTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly IMqttConnectionFactory _connectionFactory;
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttTriggerAttribute"/>.
        /// </summary>
        /// <param name="connectionFactory">the connection factory.</param>
        /// <param name="loggerFactory">The loggerFactory.</param>
        /// <param name="nameResolver">The nameResolver.</param>
        internal MqttTriggerAttributeBindingProvider(IMqttConnectionFactory connectionFactory, ILoggerFactory loggerFactory, INameResolver nameResolver)
        {
            _connectionFactory = connectionFactory;
            _nameResolver = nameResolver;
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
            try
            {
                var mqttTriggerBinding = GetMqttTriggerBinding(context.Parameter, mqttTriggerAttribute);

                _logger.LogDebug($"Succesfully created binding for parameter '{context.Parameter.Name}'");

                return Task.FromResult(mqttTriggerBinding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception while binding trigger '{context.Parameter.Name}'");
                throw;
            }
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

        private ITriggerBinding GetMqttTriggerBinding(ParameterInfo parameter, MqttTriggerAttribute mqttTriggerAttribute)
        {
            var topicFilters = new List<MqttTopicFilter>();
            
            var mqttConnection = _connectionFactory.GetMqttConnection(mqttTriggerAttribute);
            try
            {
                if (mqttTriggerAttribute.TopicFilter != null)
                {
                    topicFilters.Add(mqttTriggerAttribute.TopicFilter);
                }
                else
                {
                    topicFilters.AddRange(mqttTriggerAttribute.TopicStrings.Select(t =>
                    {
                        var topicString = (mqttTriggerAttribute.MqttConfigCreatorType != null) ? _nameResolver.ResolveWholeString(t) : t;
                        return new MqttTopicFilter() { Topic = topicString, QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce };
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }

            return new MqttTriggerBinding(parameter, mqttConnection, topicFilters.ToArray(), _logger);
        }
    }
}
