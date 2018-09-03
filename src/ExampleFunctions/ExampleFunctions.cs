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
            logger.LogInformation($"Message for topic {message.Topic}: {bodyString}");
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

        /// <summary>
        /// This sample function shows how to run a function based on a timer trigger and then publishing to a MQTT topic
        /// In this case the ICollector construct is used to output multiple results.
        /// The Mqtt attribute is configured to use another connectionstring provided in the settings
        /// </summary>
        [FunctionName("TimerFunction")]
        public static void TimerFunction( 
            [TimerTrigger("0 * * * * *")]TimerInfo timerInfo,
            [Mqtt] ICollector<IMqttMessage> outMessages,
            ILogger logger)
        {
            var body = Encoding.UTF8.GetBytes($"It is currently {DateTime.UtcNow:g}");

            outMessages.Add(new MqttMessage("topic/one", body, MqttQualityOfServiceLevel.AtLeastOnce, true));
            outMessages.Add(new MqttMessage("topic/two", body, MqttQualityOfServiceLevel.AtLeastOnce, true));
        }
    }
}
