using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client.Connecting;
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
            var messageProcessor = new Mock<IProcesMqttMessage>();

            mockMqttClientFactory
                .Setup(m => m.CreateManagedMqttClient())
                .Returns(mockManagedMqttClient.Object);

            mockManagedMqttClient
                .Setup(m => m.StartAsync(It.Is<IManagedMqttClientOptions>(y => y == mockManagedMqttClientOptions.Object)))
                .Returns(Task.CompletedTask);

            messageProcessor.Setup(x => x.OnMessage(It.IsAny<MqttMessageReceivedEventArgs>())).Returns(Task.CompletedTask);

            var config = new MqttConfiguration("CustomConfig", mockManagedMqttClientOptions.Object);

            var mqttConnection = new MqttConnection(mockMqttClientFactory.Object, config, _mockLogger.Object);

            // Act
            await mqttConnection.StartAsync(messageProcessor.Object);
            await mqttConnection.HandleConnectedAsync(new MqttClientConnectedEventArgs(new MqttClientAuthenticateResult {
                IsSessionPresent = true,
                ResultCode = MqttClientConnectResultCode.Success
            })); 

            // Assert 
            Assert.Equal(ConnectionState.Connected, mqttConnection.ConnectionState);
            mockMqttClientFactory.VerifyAll();
            mockManagedMqttClient.VerifyAll();
        }

        [Fact]
        public async Task NewMessageIsProcessed()
        {
            // Arrange 
            var mockManagedMqttClient = new Mock<IManagedMqttClient>();
            var mockManagedMqttClientOptions = new Mock<IManagedMqttClientOptions>();
            var mockMqttClientFactory = new Mock<IManagedMqttClientFactory>();
            var messageProcessor = new Mock<IProcesMqttMessage>();

            mockMqttClientFactory
                .Setup(m => m.CreateManagedMqttClient())
                .Returns(mockManagedMqttClient.Object);

            messageProcessor.Setup(x => x.OnMessage(It.IsAny<MqttMessageReceivedEventArgs>())).Returns(Task.CompletedTask);

            var config = new MqttConfiguration("CustomConfig", mockManagedMqttClientOptions.Object);
            var mqttConnection = new MqttConnection(mockMqttClientFactory.Object, config, _mockLogger.Object); 
           
            // Act 
            await mqttConnection.StartAsync(messageProcessor.Object);
            await mqttConnection.HandleApplicationMessageReceivedAsync( new MqttApplicationMessageReceivedEventArgs("ClientId", DefaultMessage)); 

            // Assert 
            messageProcessor.Verify(x => x.OnMessage(It.Is<MqttMessageReceivedEventArgs>(y => y.Message.Topic == DefaultMessage.Topic)));
            messageProcessor.Verify(x => x.OnMessage(It.Is<MqttMessageReceivedEventArgs>(y => y.Message.Retain == DefaultMessage.Retain)));
            messageProcessor.Verify(x => x.OnMessage(It.Is<MqttMessageReceivedEventArgs>(y => y.Message.QosLevel.ToString() == DefaultMessage.QualityOfServiceLevel.ToString())));
            messageProcessor.Verify(x => x.OnMessage(It.Is<MqttMessageReceivedEventArgs>(y => y.Message.GetMessage() == DefaultMessage.Payload)));
        }
    } 
}
