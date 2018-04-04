using System;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class MqttListenerTests
    {
        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
        private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;


        [Fact]
        public async Task StartAsyncSubscribesToTopics()
        {
            // Arrange 
            var mockManagedMqttClient = new Mock<IManagedMqttClient>();
            var mockManagedMqttClientOptions = new Mock<IManagedMqttClientOptions>();
            var mqttConfiguration = new MqttConfiguration(mockManagedMqttClientOptions.Object, new[] { new TopicFilter("test/topic", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) });
            var mockMqttClientFactory = new Mock<IMqttClientFactory>();
            var mockTriggeredFunctionExecutor = new Mock<ITriggeredFunctionExecutor>();

            mockManagedMqttClient
                .Setup(m => m.SubscribeAsync(It.Is<TopicFilter[]>(y => y == mqttConfiguration.Topics)))
                .Returns(Task.CompletedTask);

            mockManagedMqttClient
                .Setup(m => m.StartAsync(It.Is<IManagedMqttClientOptions>(y => y == mqttConfiguration.Options)))
                .Returns(Task.CompletedTask);

            mockMqttClientFactory
                .Setup(m => m.CreateManagedMqttClient())
                .Returns(mockManagedMqttClient.Object);

            var mqttListener = new MqttListener(mockMqttClientFactory.Object, mqttConfiguration, mockTriggeredFunctionExecutor.Object, _mockLogger.Object);

            // Act
            await mqttListener.StartAsync(_cancellationToken).ConfigureAwait(false);

            // Assert 
            mockMqttClientFactory.Verify(x => x.CreateManagedMqttClient(), Times.Once());
            mockManagedMqttClient.VerifyAll();
        }

        [Fact]
        public async Task NewMessageInvokesFunction()
        {
            // Arrange 
            var mockManagedMqttClient = new Mock<IManagedMqttClient>();
            var mockManagedMqttClientOptions = new Mock<IManagedMqttClientOptions>();
            mockManagedMqttClientOptions.Setup(x => x.ClientOptions.ClientId).Returns(Guid.NewGuid().ToString());
            var mqttConfiguration = new MqttConfiguration(mockManagedMqttClientOptions.Object, new[] { new TopicFilter("test/topic", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) });
            var mockMqttClientFactory = new Mock<IMqttClientFactory>();
            var mockTriggeredFunctionExecutor = new Mock<ITriggeredFunctionExecutor>();

            mockManagedMqttClient
                .Setup(m => m.SubscribeAsync(It.Is<TopicFilter[]>(y => y == mqttConfiguration.Topics)))
                .Returns(Task.CompletedTask);
            mockManagedMqttClient
                .Setup(m => m.StartAsync(It.Is<IManagedMqttClientOptions>(y => y == mqttConfiguration.Options)))
                .Returns(Task.CompletedTask);

            mockMqttClientFactory
                .Setup(m => m.CreateManagedMqttClient())
                .Returns(mockManagedMqttClient.Object);

            var mqttListener = new MqttListener(mockMqttClientFactory.Object, mqttConfiguration, mockTriggeredFunctionExecutor.Object, _mockLogger.Object);
            var testMessage = new MqttApplicationMessageBuilder()
                 .WithPayload("{}")
                 .WithTopic("123")
                 .WithAtLeastOnceQoS()
                 .Build();

            // Act
            await mqttListener.StartAsync(_cancellationToken).ConfigureAwait(false);
            mockManagedMqttClient.Raise(x => x.ApplicationMessageReceived += null, new MqttApplicationMessageReceivedEventArgs(mqttConfiguration.Options.ClientOptions.ClientId, testMessage));

            // Assert 
            mockTriggeredFunctionExecutor.Verify(x => x.TryExecuteAsync(It.IsAny<TriggeredFunctionData>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task InitializationFailsThrowsRightException()
        {
            // Arrange 
            var mockManagedMqttClient = new Mock<IManagedMqttClient>();
            var mockManagedMqttClientOptions = new Mock<IManagedMqttClientOptions>();
            var mqttConfiguration = new MqttConfiguration(mockManagedMqttClientOptions.Object, new[] { new TopicFilter("test/topic", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) });
            var mockMqttClientFactory = new Mock<IMqttClientFactory>();
            var mockTriggeredFunctionExecutor = new Mock<ITriggeredFunctionExecutor>();
            
            mockMqttClientFactory
                .Setup(m => m.CreateManagedMqttClient())
                .Throws<Exception>();

            var mqttListener = new MqttListener(mockMqttClientFactory.Object, mqttConfiguration, mockTriggeredFunctionExecutor.Object, _mockLogger.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<MqttListenerInitializationException>(async () => await mqttListener.StartAsync(_cancellationToken));
        }
    }
}
