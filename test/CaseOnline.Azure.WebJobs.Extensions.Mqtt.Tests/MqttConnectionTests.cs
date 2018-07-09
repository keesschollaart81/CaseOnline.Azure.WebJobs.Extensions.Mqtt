using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class MqttConnectionTests
    {
        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
        private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;

        private MqttApplicationMessage DefaultMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test/topic")
                    .WithPayload("{ \"test\":\"case\" }")
                    .WithAtLeastOnceQoS()
                    .Build();

        [Fact]
        public async Task StartAsyncSubscribesToTopics()
        {
            // Arrange 
            var mockManagedMqttClient = new Mock<IManagedMqttClient>();
            var mockManagedMqttClientOptions = new Mock<IManagedMqttClientOptions>();
            var mockMqttClientFactory = new Mock<IManagedMqttClientFactory>();

            mockMqttClientFactory
                .Setup(m => m.CreateManagedMqttClient())
                .Returns(mockManagedMqttClient.Object);

            mockManagedMqttClient
                .Setup(m => m.StartAsync(It.Is<IManagedMqttClientOptions>(y => y == mockManagedMqttClientOptions.Object)))
                .Returns(Task.CompletedTask);

            var config = new MqttConfiguration("CustomConfig", mockManagedMqttClientOptions.Object);

            var mqttConnection = new MqttConnection(mockMqttClientFactory.Object, config, _mockLogger.Object);

            // Act
            await mqttConnection.StartAsync();
            mockManagedMqttClient.Raise(x => x.Connected += null, new MqttClientConnectedEventArgs(true));

            // Assert 
            Assert.Equal(ConnectionState.Connected, mqttConnection.ConnectionState);
            mockMqttClientFactory.VerifyAll();
            mockManagedMqttClient.VerifyAll();
        }

        [Fact]
        public async Task NewMessageIsProcessedWell()
        {
            // Arrange 
            var mockManagedMqttClient = new Mock<IManagedMqttClient>();
            var mockManagedMqttClientOptions = new Mock<IManagedMqttClientOptions>();
            var mockMqttClientFactory = new Mock<IManagedMqttClientFactory>();

            mockMqttClientFactory
                .Setup(m => m.CreateManagedMqttClient())
                .Returns(mockManagedMqttClient.Object);

            var config = new MqttConfiguration("CustomConfig", mockManagedMqttClientOptions.Object);
            var mqttConnection = new MqttConnection(mockMqttClientFactory.Object, config, _mockLogger.Object);

            IMqttMessage receivedMessage = null;
            mqttConnection.OnMessageEventHandler += (MqttMessageReceivedEventArgs arg) =>
            {
                receivedMessage = arg.Message;
                return Task.CompletedTask;
            };

            // Act 
            await mqttConnection.StartAsync();
            mockManagedMqttClient.Raise(x => x.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs("ClientId", DefaultMessage));

            // Assert 
            Assert.NotNull(receivedMessage);
            Assert.Equal(DefaultMessage.Topic, receivedMessage.Topic);
            Assert.Equal(DefaultMessage.Retain, receivedMessage.Retain);
            Assert.Equal(DefaultMessage.QualityOfServiceLevel.ToString(), receivedMessage.QosLevel.ToString());
            Assert.Equal(DefaultMessage.Payload, receivedMessage.GetMessage());
        }
    }
}
