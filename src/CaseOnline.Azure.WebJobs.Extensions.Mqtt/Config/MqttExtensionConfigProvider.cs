using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttExtensionConfigProvider : IExtensionConfigProvider
    {
        private INameResolver _nameResolver;

        public void Initialize(ExtensionConfigContext context)
        {
            context.Trace.Info("MqttExtensionConfigProvider.Initialize() called!");

            var logger = context.Config.LoggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));
            logger.LogWarning("Logger not working?");

            _nameResolver = context.Config.GetService<INameResolver>();

            context.Config.RegisterBindingExtension(new MqttTriggerAttributeBindingProvider(_nameResolver, logger, context.Trace));
        }
    }
}
