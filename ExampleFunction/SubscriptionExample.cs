using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using System.Text;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;

namespace ExampleFunction
{
    public static class SubscriptionExample
    {
        [FunctionName("SubscriptionExample")]
        public static void Run([MqttTrigger("192.168.2.2", "cmnd/keukenlamp/POWER", "kees", "")]PublishedMqttMessage message, TraceWriter log)
        {
            var envelope = Encoding.UTF8.GetString(message.Message);
            log.Info($"Received message from topic: {message.Topic} with content {envelope}");
        }
    }

    public class Test : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            Console.WriteLine("test");
            ILogger logger = context.Config.LoggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Mqtt"));
            context.Config.RegisterBindingExtension(new MqttTriggerAttributeBindingProvider(context.Config.NameResolver, logger));
            //var rule2 = context.AddBindingRule<FileTriggerAttribute>();
            //rule2.BindToTrigger(new MqttTriggerAttributeBindingProvider(context.Config.NameResolver, logger));

        }
    }
}
