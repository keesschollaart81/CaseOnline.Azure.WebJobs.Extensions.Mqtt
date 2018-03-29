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

        [Fact]
        public async Task StartAsyncSubscribesToTopics()
        {
            // Arrange 
            var mockManagedMqttClient = new Mock<IManagedMqttClient>();
            var mockManagedMqttClientOptions = new Mock<IManagedMqttClientOptions>();
            var mqttConfiguration = new MqttConfiguration(mockManagedMqttClientOptions.Object, new[] { new TopicFilter("test/topic", MqttQualityOfServiceLevel.AtLeastOnce) });
            var mockMqttClientFactory = new Mock<IMqttClientFactory>();
            var mockTriggeredFunctionExecutor = new Mock<ITriggeredFunctionExecutor>(); 
            var cancellationToken = new CancellationTokenSource().Token;

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
            await mqttListener.StartAsync(cancellationToken).ConfigureAwait(false);

            // Assert 
            mockMqttClientFactory.Verify(x => x.CreateManagedMqttClient(), Times.Once());
            mockManagedMqttClient.VerifyAll();
        }
    }
}
