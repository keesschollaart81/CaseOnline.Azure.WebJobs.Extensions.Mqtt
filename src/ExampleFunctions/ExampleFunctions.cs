using System;
using Microsoft.Azure.WebJobs;
using System.Text;
using Microsoft.Extensions.Logging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;

namespace ExampleFunctions
{
    public static class ExampleFunctions
    {
        [FunctionName("SimpleFunction")]
        public static void SimpleFunction(
            [MqttTrigger("devices/testdevice/#")] PublishedMqttMessage message,
            ILogger logger)
        {
            var body = message.GetMessage();
            var bodyString = Encoding.UTF8.GetString(body);
            logger.LogInformation($"{DateTime.Now:g} Message for topic {message.Topic}: {bodyString}");
        }

        [FunctionName("AdvancedFunction")]
        public static void AdvancedFunction(
            [MqttTrigger(typeof(ExampleMqttConfigProvider))]PublishedMqttMessage message,
            ILogger log)
        {
            var body = Encoding.UTF8.GetString(message.GetMessage());

            log.LogInformation($"Advanced: message from topic {message.Topic} body: {body}");
        }
    }
}
