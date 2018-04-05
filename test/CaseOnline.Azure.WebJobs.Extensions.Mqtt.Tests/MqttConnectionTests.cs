using System;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class MqttConnectionTests
    {
        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
        private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;

        private IMqttMessage DefaultMessage = new MqttMessage("test/topic", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true);

        [Fact]
        public async Task StartAsyncSubscribesToTopics()
        {
            // Arrange 
            var mockManagedMqttClient = new Mock<IManagedMqttClient>();
            var mockManagedMqttClientOptions = new Mock<IManagedMqttClientOptions>();
            var mockMqttClientFactory = new Mock<IMqttClientFactory>();

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

            // Assert 
            mockMqttClientFactory.VerifyAll();
            mockManagedMqttClient.VerifyAll();
        }
         
    }
}
