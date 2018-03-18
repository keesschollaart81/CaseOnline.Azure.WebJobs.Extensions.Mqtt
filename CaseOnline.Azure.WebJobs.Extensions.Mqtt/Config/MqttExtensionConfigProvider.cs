using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttExtensionConfigProvider : IExtensionConfigProvider
    {
        private INameResolver _nameResolver; 
        
        public void Initialize(ExtensionConfigContext context)
        {
            context.Trace.Info("MqttExtensionConfigProvider.Initialize() called!");

            _nameResolver = context.Config.GetService<INameResolver>();
            
            context.Config.RegisterBindingExtension(new MqttTriggerAttributeBindingProvider(_nameResolver, context.Trace));
        }
    }
}
