using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    /// <summary>
    /// Registers the <see cref="MqttTriggerAttribute"/> binding.
    /// </summary>
    public class MqttExtensionConfigProvider : IExtensionConfigProvider
    {
        private static MqttConnectionFactory mqttConnectionFactory;

        internal static MqttConnectionFactory MqttConnectionFactory { get => mqttConnectionFactory; }

        /// <summary>
        /// Initializes the extension configuration provider.
        /// </summary>
        /// <param name="context">The extension configuration context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            var logger = context.Config.LoggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));

            var nameResolver = context.Config.GetService<INameResolver>();

            mqttConnectionFactory = new MqttConnectionFactory(logger, new MqttFactory(), nameResolver);
            
            var mqttAttributeBindingRule = context.AddBindingRule<MqttAttribute>();
            mqttAttributeBindingRule.BindToCollector((attr) => new MqttMessageCollector(attr, MqttConnectionFactory.GetMqttConnectionAsync(attr).Result));

            context.Config.RegisterBindingExtension(new MqttTriggerAttributeBindingProvider(nameResolver, MqttConnectionFactory, logger));
            
            // todo: in later release of the Functions SDK (currently beta 25) replace the line above with the two below
            //var mqttTriggerAttributeBindingRule = context.AddBindingRule<MqttTriggerAttribute>();
            //mqttTriggerAttributeBindingRule.BindToTrigger<IMqttMessage>(new MqttTriggerAttributeBindingProvider(nameResolver, _mqttConnectionFactory, logger));
        }
    }
}
