using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers;
using Microsoft.Azure.WebJobs;
using MQTTnet;
using MQTTnet.Server;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.EndToEnd
{
    public class OutputBindingTests : EndToEndTestBase
    {
        [Fact]
        public async Task SimpleMessageIsPublished()
        {
            MqttApplicationMessage mqttApplicationMessage = null;

            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var mqttClient = await MqttClientHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<SimpleOutputIsPublishedTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttClient.SubscribeAsync("test/topic");
                mqttClient.OnMessage += (object sender, OnMessageEventArgs e) => mqttApplicationMessage = e.ApplicationMessage;

                await jobHost.CallAsync(nameof(SimpleOutputIsPublishedTestFunction.Testert));

                await WaitFor(() => mqttApplicationMessage != null);
            }

            Assert.NotNull(mqttApplicationMessage);
        }

        public class SimpleOutputIsPublishedTestFunction
        {
            public static void Testert([Mqtt]out IMqttMessage mqttMessage)
            {
                mqttMessage = new MqttMessage("test/topic", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true);
            }
        }

        [Fact]
        public async Task TriggerAndOutputReuseConnection()
        {
            var mqttApplicationMessages = new List<MqttApplicationMessage>();
            var counnections = 0;
            var options = new MqttServerOptionsBuilder()
                .WithConnectionValidator(x =>
                {
                    counnections += (x.ClientId != "IntegrationTest") ? 1 : 0;

                    Debug.WriteLine($"ClientId:{x.ClientId}");
                })
                .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger, options))
            using (var mqttClient = await MqttClientHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<TriggerAndOutputWithSameConnectionTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttClient.SubscribeAsync("test/outtopic");
                await mqttClient.SubscribeAsync("test/outtopic2");
                mqttClient.OnMessage += (object sender, OnMessageEventArgs e) => mqttApplicationMessages.Add(e.ApplicationMessage);

                await mqttServer.PublishAsync(DefaultMessage);

                await WaitFor(() => TriggerAndOutputWithSameConnectionTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, TriggerAndOutputWithSameConnectionTestFunction.CallCount);
            Assert.Equal(1, counnections);

            Assert.Equal(2, mqttApplicationMessages.Count);
            Assert.Contains(mqttApplicationMessages, x => x.Topic == "test/outtopic");
            Assert.Contains(mqttApplicationMessages, x => x.Topic == "test/outtopic2");

            var bodyString = Encoding.UTF8.GetString(mqttApplicationMessages.First().Payload);
            Assert.Equal("{\"test\":\"message\"}", bodyString);
        }

        public class TriggerAndOutputWithSameConnectionTestFunction
        {
            public static IMqttMessage LastIncomingMessage;
            public static int CallCount = 0;

            public static void Testert(
                [MqttTrigger("test/topic")]IMqttMessage incomgingMessage,
                [Mqtt]out IMqttMessage outGoingMessage,
                [Mqtt]out IMqttMessage outGoingMessage2)
            {
                LastIncomingMessage = incomgingMessage;
                Interlocked.Increment(ref CallCount);

                var updatedBody = Encoding.UTF8.GetBytes("{\"test\":\"message\"}");
                outGoingMessage = new MqttMessage("test/outtopic", updatedBody, MqttQualityOfServiceLevel.AtLeastOnce, true);
                outGoingMessage2 = new MqttMessage("test/outtopic2", updatedBody, MqttQualityOfServiceLevel.AtLeastOnce, true);
            }
        }

        [Fact]
        public async Task TriggerAndOutputUseDifferentConnection()
        {
            MqttApplicationMessage mqttApplicationMessage = null;

            var connectionsCountServer1 = 0;
            var optionsServer1 = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(1337)
                .WithConnectionValidator(x => connectionsCountServer1 += (x.ClientId != "IntegrationTest") ? 1 : 0)
                .Build();

            var connectionsCountServer2 = 0;
            var optionsServer2 = new MqttServerOptionsBuilder()
                .WithConnectionValidator(x => connectionsCountServer2 += (x.ClientId != "IntegrationTest") ? 1 : 0)
                .Build();

            using (var mqttServer1 = await MqttServerHelper.Get(_logger, optionsServer1))
            using (var mqttServer2 = await MqttServerHelper.Get(_logger, optionsServer2))
            using (var mqttClientForServer2 = await MqttClientHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<TriggerAndOutputWithDifferentConnectionTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttClientForServer2.SubscribeAsync("test/outtopic");
                mqttClientForServer2.OnMessage += (object sender, OnMessageEventArgs e) => mqttApplicationMessage = e.ApplicationMessage;

                await mqttServer1.PublishAsync(DefaultMessage);

                await WaitFor(() => TriggerAndOutputWithDifferentConnectionTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, TriggerAndOutputWithDifferentConnectionTestFunction.CallCount);
            Assert.Equal(1, connectionsCountServer1);
            Assert.Equal(1, connectionsCountServer2);

            Assert.NotNull(mqttApplicationMessage);
            Assert.Equal("test/outtopic", mqttApplicationMessage.Topic);

            var bodyString = Encoding.UTF8.GetString(mqttApplicationMessage.Payload);
            Assert.Equal("{\"test\":\"message\"}", bodyString);
        }

        public class TriggerAndOutputWithDifferentConnectionTestFunction
        {
            public static IMqttMessage LastIncomingMessage;
            public static int CallCount = 0;

            public static void Testert(
                [MqttTrigger("test/topic", ConnectionString = "MqttConnectionWithCustomPort")]IMqttMessage incomgingMessage,
                [Mqtt]out IMqttMessage outGoingMessage)
            {
                LastIncomingMessage = incomgingMessage;
                CallCount++;

                var updatedBody = Encoding.UTF8.GetBytes("{\"test\":\"message\"}");
                outGoingMessage = new MqttMessage("test/outtopic", updatedBody, MqttQualityOfServiceLevel.AtLeastOnce, true);
            }
        }

        [Fact]
        public async Task ICollectorOutputsArePublished()
        {
            var mqttApplicationMessages = new List<MqttApplicationMessage>(); ;

            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var mqttClient = await MqttClientHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<ICollectorOutputIsPublishedTestFunction>.RunFor(_testLoggerProvider))
            { 
                await mqttClient.SubscribeAsync("test/outtopic");
                await mqttClient.SubscribeAsync("test/outtopic2"); 
                mqttClient.OnMessage += (object sender, OnMessageEventArgs e) => mqttApplicationMessages.Add(e.ApplicationMessage);

                await jobHost.CallAsync(nameof(ICollectorOutputIsPublishedTestFunction.Testert));

                await WaitFor(() => ICollectorOutputIsPublishedTestFunction.CallCount >= 1);

                await WaitFor(() => mqttApplicationMessages.Count > 0);
            }

            Assert.Equal(1, ICollectorOutputIsPublishedTestFunction.CallCount);

            Assert.Equal(2, mqttApplicationMessages.Count);
            Assert.Contains(mqttApplicationMessages, x => x.Topic == "test/outtopic");
            Assert.Contains(mqttApplicationMessages, x => x.Topic == "test/outtopic2");

            var bodyString = Encoding.UTF8.GetString(mqttApplicationMessages.First().Payload);
            Assert.Equal("", bodyString);
        }

        public class ICollectorOutputIsPublishedTestFunction
        {
            public static int CallCount = 0;

            public static void Testert([Mqtt] ICollector<IMqttMessage> mqttMessages)
            {
                mqttMessages.Add(new MqttMessage("test/outtopic", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true));
                mqttMessages.Add(new MqttMessage("test/outtopic2", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true));
                Interlocked.Increment(ref CallCount);
            }
        } 
    }
}
