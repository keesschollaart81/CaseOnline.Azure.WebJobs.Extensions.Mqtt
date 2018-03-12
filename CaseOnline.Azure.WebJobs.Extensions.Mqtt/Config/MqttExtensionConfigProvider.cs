using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttExtensionConfigProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            context.Trace.Info("MqttExtensionConfigProvider.Initialize() called!");
            context.Config.RegisterBindingExtension(new MqttTriggerAttributeBindingProvider(context.Config.NameResolver, context.Trace));
        } 
    } 
}
