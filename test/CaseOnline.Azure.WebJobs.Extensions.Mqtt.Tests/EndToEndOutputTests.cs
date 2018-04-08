using System;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class EndToEndOutputTests
    {
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;

        public EndToEndOutputTests()
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new TestLoggerProvider());
            _logger = _loggerFactory.CreateLogger("EndToEndTests");
        }

        [Fact]
        public async Task SimpleMessageIsPublished()
        {
            MqttApplicationMessage mqttApplicationMessage = null;

            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var mqttClient = await MqttClientHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<SimpleOutputIsPublishedTestFunction>.RunFor(_loggerFactory))
            {
                await mqttClient.SubscribeAsync("test/topic");
                mqttClient.OnMessage += (object sender, OnMessageEventArgs e) => mqttApplicationMessage = e.ApplicationMessage;

                await jobHost.CallAsync(nameof(SimpleOutputIsPublishedTestFunction.Testert));

                await jobHost.WaitFor(() => mqttApplicationMessage != null);
            }

            Assert.NotNull(mqttApplicationMessage);
        }

        public class SimpleOutputIsPublishedTestFunction
        {
            public static void Testert([Mqtt("test/topic")]out IMqttMessage mqttMessage)
            {
                mqttMessage = new MqttMessage("test/topic", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true);
            }
        }
    }
}
