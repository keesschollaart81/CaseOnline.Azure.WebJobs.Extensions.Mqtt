using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    //public class MqttExtensionConfigProvider : IExtensionConfigProvider
    //{
    //    public void Initialize(ExtensionConfigContext context)
    //    {
    //        Console.WriteLine("test");
    //        ILogger logger = context.Config.LoggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));
    //        context.Config.RegisterBindingExtension(new MqttTriggerAttributeBindingProvider(context.Config.NameResolver, logger));
    //        //var rule2 = context.AddBindingRule<FileTriggerAttribute>();
    //        //rule2.BindToTrigger(new MqttTriggerAttributeBindingProvider(context.Config.NameResolver, logger));

    //    }
    //}
}
