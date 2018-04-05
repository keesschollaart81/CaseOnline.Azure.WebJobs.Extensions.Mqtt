using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class EndToEndOutputTests
    {
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;

        private MqttApplicationMessage DefaultMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test/topic")
                    .WithPayload("{ \"test\":\"case\" }")
                    .WithAtLeastOnceQoS()
                    .Build();

        public EndToEndOutputTests()
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new TestLoggerProvider());
            _logger = _loggerFactory.CreateLogger("EndToEndTests");
        }

        [Fact]
        public async Task SimpleMessageIsReceived()
        {
            MqttApplicationMessage mqttApplicationMessage = null;
            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var jobHost = await JobHostHelper.RunFor<SimpleOutputIsPublishedTestFunction>(_loggerFactory))
            {
                mqttServer.OnMessage += (object sender, OnMessageEventArgs e) =>
                {
                    mqttApplicationMessage = e.ApplicationMessage;
                };
                await mqttServer.SubscribeAsync("test/topic");

                var method = typeof(SimpleOutputIsPublishedTestFunction).GetMethod(nameof(SimpleOutputIsPublishedTestFunction.Testert));
                await jobHost.CallAsync(method, new { });

                await jobHost.WaitFor(() => mqttApplicationMessage != null);
            }

            Assert.NotNull(mqttApplicationMessage);
        }

        public class SimpleOutputIsPublishedTestFunction
        {
            [FunctionName("Testert")]
            [NoAutomaticTrigger()]
            public static void Testert([Mqtt("test/topic")]out IMqttMessage mqttMessage)
            {
                mqttMessage = new MqttMessage("test/topic", new byte[] { }, Messaging.MqttQualityOfServiceLevel.AtLeastOnce, true);
            }
        }
    }
}
