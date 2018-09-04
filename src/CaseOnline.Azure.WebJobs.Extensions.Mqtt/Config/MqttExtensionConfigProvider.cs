using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging; 

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    /// <summary>
    /// Registers the <see cref="MqttTriggerAttribute"/> binding.
    /// </summary>
    [Extension("Mqtt")]
    public class MqttExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly INameResolver _nameResolver;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMqttConnectionFactory _mqttConnectionFactory;

        public MqttExtensionConfigProvider(INameResolver nameResolver, ILoggerFactory loggerFactory, IMqttConnectionFactory mqttConnectionFactory)
        {
            _nameResolver = nameResolver;
            _loggerFactory = loggerFactory;
            _mqttConnectionFactory = mqttConnectionFactory;
        }

        /// <summary>
        /// Initializes the extension configuration provider.
        /// </summary>
        /// <param name="context">The extension configuration context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            var mqttAttributeBindingRule = context.AddBindingRule<MqttAttribute>();
            mqttAttributeBindingRule.BindToCollector((attr) =>
            {
                return new MqttMessageCollector(attr, _mqttConnectionFactory.GetMqttConnection(attr));
            });

            var bindingProvider = new MqttTriggerAttributeBindingProvider(_nameResolver, _mqttConnectionFactory, _loggerFactory);
            context.AddBindingRule<MqttTriggerAttribute>()
                .BindToTrigger(bindingProvider);
        }
    }
}
