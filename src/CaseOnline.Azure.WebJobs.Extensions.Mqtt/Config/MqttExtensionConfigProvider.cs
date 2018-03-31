using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    /// <summary>
    /// Registers the <see cref="MqttTriggerAttribute"/> binding.
    /// </summary>
    public class MqttExtensionConfigProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Initializes the extension configuration provider.
        /// </summary>
        /// <param name="context">The extension configuration context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            var logger = context.Config.LoggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));

            var nameResolver = context.Config.GetService<INameResolver>();

            context.Config.RegisterBindingExtension(new MqttTriggerAttributeBindingProvider(nameResolver, logger));
        }
    }
}
