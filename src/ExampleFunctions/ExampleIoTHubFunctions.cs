using System;
using Microsoft.Azure.WebJobs;
using System.Text;
using Microsoft.Extensions.Logging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using System.Linq;

namespace ExampleFunctions
{
    public static class ExampleIoTHubFunctions
    {
        // Attention: this function is disabled because it requires an Azure IoT Hub and a corresponding connectionstring
        // To get it working, remove the [Disable] attribute on the trigger and a the "IoTHubConnectionString" to the settings
        // More on IoT Hub: https://github.com/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt/wiki/Azure-IoT-Hub

        [FunctionName("CloudToDeviceMessages")]
        public static void CloudToDeviceMessages(
            [MqttTrigger("devices/testdevice/messages/devicebound/#", "$iothub/methods/POST/#", ConnectionString = "IoTHubConnectionString"), Disable]IMqttMessage message,
            [Mqtt(ConnectionString = "IoTHubConnectionString"), Disable] out IMqttMessage response,
            ILogger logger)
        {
            var body = message.GetMessage();
            var bodyString = Encoding.UTF8.GetString(body);
            logger.LogInformation($"{DateTime.Now:g} Message for topic {message.Topic}: {bodyString}");

            if (message.Topic.Contains("methods"))
            {
                response = CloudToDeviceMethodCall(message.Topic, bodyString);
            }
            else
            {
                response = null;
            }
        }

        private static IMqttMessage CloudToDeviceMethodCall(string topic, string message)
        {
            var requestId = topic.Split('=').Last();

            var responseBodyString = "{}";
            var responseBodyBytes = Encoding.UTF8.GetBytes(responseBodyString);

            return new MqttMessage($"$iothub/methods/res/200/?$rid={requestId}", responseBodyBytes, MqttQualityOfServiceLevel.AtLeastOnce, true);
        }
    }
}
