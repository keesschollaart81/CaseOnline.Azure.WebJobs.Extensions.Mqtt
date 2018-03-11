using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    public class MqttTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly ILogger _logger;

        public MqttTriggerAttributeBindingProvider(INameResolver nameResolver, ILogger logger)
        {
            _nameResolver = nameResolver;
            _logger = logger;
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

            var config = new MqttConfiguration(
                mqttTriggerAttribute.ServerUrl,
                mqttTriggerAttribute.Topic,
                mqttTriggerAttribute.Username,
                mqttTriggerAttribute.Password,
                mqttTriggerAttribute.ClientId
            );

            return Task.FromResult<ITriggerBinding>(new MqttTriggerBinding(parameter, mqttTriggerAttribute, config, _logger));
        }
    }
}
