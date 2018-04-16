using System;
using Microsoft.Azure.WebJobs;
using System.Text;
using Microsoft.Extensions.Logging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using ExampleFunction.AdvancedConfig;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;

namespace ExampleFunctions
{
    public static class ExampleFunctions
    {
        [FunctionName("SimpleFunction")]
        public static void SimpleFunction(
            [MqttTrigger("owntracks/#")] IMqttMessage message,
            [Mqtt] out IMqttMessage outMessage,
            ILogger logger)
        {
            var body = message.GetMessage();
            var bodyString = Encoding.UTF8.GetString(body);
            logger.LogInformation($"{DateTime.Now:g} Message for topic {message.Topic}: {bodyString}");
            outMessage = new MqttMessage("testtopic/out", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true);
        }

        [FunctionName("AdvancedFunction")]
        public static void AdvancedFunction(
            [MqttTrigger(typeof(ExampleMqttConfigProvider), "testtopic/#")]IMqttMessage message,
            ILogger log)
        {
            var body = Encoding.UTF8.GetString(message.GetMessage());

            log.LogInformation($"Advanced: message from topic {message.Topic} body: {body}");
        }
    }
}
