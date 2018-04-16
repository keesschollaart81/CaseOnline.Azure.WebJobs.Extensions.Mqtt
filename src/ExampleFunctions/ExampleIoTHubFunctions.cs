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
        // Attention: these functions are disabled in this example project because they require an Azure IoT Hub and a corresponding connectionstring
        // To get these working, remove the [Disable] attribute on the triggers and a the IoTHubConnectionString to the settings
        // More on IoT Hub: https://github.com/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt/wiki/Azure-IoT-Hub

        [FunctionName("CloudToDeviceMessages")]
        public static void CloudToDeviceMessages([MqttTrigger("devices/testdevice/messages/devicebound/#", ConnectionString = "IoTHubConnectionString"), Disable]IMqttMessage message, ILogger logger)
        {
            var body = message.GetMessage();
            var bodyString = Encoding.UTF8.GetString(body);
            logger.LogInformation($"{DateTime.Now:g} Message for topic {message.Topic}: {bodyString}");
        }

        [FunctionName("ReplyToMethodCalls")]
        public static void ReplyToMethodCalls(
            [MqttTrigger("$iothub/methods/POST/#", ConnectionString = "IoTHubConnectionString"), Disable]IMqttMessage request,
            [Mqtt] out IMqttMessage response,
            ILogger logger)
        {
            var body = request.GetMessage();
            var bodyString = Encoding.UTF8.GetString(body);

            logger.LogInformation($"{DateTime.Now:g} Request for topic {request.Topic}: {bodyString}");

            var requestId = request.Topic.Split('=').Last();

            var responseBodyString = "{}";
            var responseBodyBytes = Encoding.UTF8.GetBytes(responseBodyString);

            response = new MqttMessage($"$iothub/methods/res/200/?$rid={requestId}", responseBodyBytes, MqttQualityOfServiceLevel.AtLeastOnce, true);
        }
    }
}
