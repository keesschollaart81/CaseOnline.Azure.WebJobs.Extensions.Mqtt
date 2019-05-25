using System;
using Microsoft.Azure.WebJobs;
using System.Text;
using Microsoft.Extensions.Logging;
using ExampleFunction.AdvancedConfig;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;

namespace ExampleFunctions
{
    public static class ExampleFunctions
    {
        /// <summary>
        /// This sample functions shows how to subscribe to a topic using the default connectionstring ("MqttConnection")
        /// After logging the contents of the incoming message it also publishes a message to the "testtopic/out" topic using the same connection
        /// </summary> 
        [FunctionName("SimpleFunction")]
        public static void SimpleFunction(
            [MqttTrigger("test/#")] IMqttMessage message,
            [Mqtt] out IMqttMessage outMessage,
            ILogger logger)
        {
            var body = message.GetMessage();
            var bodyString = Encoding.UTF8.GetString(body);
            logger.LogInformation($"Message for topic {message.Topic}: {bodyString}");
            outMessage = new MqttMessage("testtopic/out", Encoding.UTF8.GetBytes("Hi!"), MqttQualityOfServiceLevel.AtLeastOnce, true);
        }

        /// <summary>
        /// This sample function shows how to trigger a function based on a custom configurated Mqtt Connection
        /// </summary>
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
