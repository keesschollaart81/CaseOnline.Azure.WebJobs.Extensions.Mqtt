using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class MqttListenerTests
    {
        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
        private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;

        private IMqttMessage DefaultMessage = new MqttMessage("test/topic", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true);

        [Fact]
        public async Task StartAsyncSubscribesToTopics()
        {
            // Arrange 
            var mockMqttConnection = new Mock<IMqttConnection>();
            var mockTriggeredFunctionExecutor = new Mock<ITriggeredFunctionExecutor>();

            mockMqttConnection
                .Setup(x => x.SubscribeAsync(It.IsAny<MqttTopicFilter[]>()))
                .Returns(Task.CompletedTask);

            mockTriggeredFunctionExecutor
                .Setup(x => x.TryExecuteAsync(It.IsAny<TriggeredFunctionData>(), It.IsAny<CancellationToken>()));

            var MqttTopicFilter = new MqttTopicFilter[] { new MqttTopicFilter() { Topic = "test/topic", QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce } };
            var mqttListener = new MqttListener(mockMqttConnection.Object, MqttTopicFilter, mockTriggeredFunctionExecutor.Object, _mockLogger.Object);

            // Act
            await mqttListener.StartAsync(_cancellationToken).ConfigureAwait(false);
            await mqttListener.OnMessage(new MqttMessageReceivedEventArgs(DefaultMessage)); // Shouldnt we be able to raise the IMqttConnection.OnMessageEventHandler??

            // Assert 
            mockMqttConnection.VerifyAll();
            mockTriggeredFunctionExecutor.VerifyAll();
        }

        [Fact]
        public async Task StopAsyncUnsubscribesToTopics()
        {
            // Arrange 
            var mockMqttConnection = new Mock<IMqttConnection>();
            var mockTriggeredFunctionExecutor = new Mock<ITriggeredFunctionExecutor>();

            mockMqttConnection
                .Setup(x => x.SubscribeAsync(It.IsAny<MqttTopicFilter[]>()))
                .Returns(Task.CompletedTask);

            mockMqttConnection
                .Setup(x => x.UnubscribeAsync(It.IsAny<string[]>()))
                .Returns(Task.CompletedTask);

            var MqttTopicFilter = new MqttTopicFilter[] { new MqttTopicFilter() { Topic = "test/topic", QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce } };
            var mqttListener = new MqttListener(mockMqttConnection.Object, MqttTopicFilter, mockTriggeredFunctionExecutor.Object, _mockLogger.Object);

            // Act
            await mqttListener.StartAsync(_cancellationToken).ConfigureAwait(false);
            await mqttListener.StopAsync(_cancellationToken).ConfigureAwait(false);

            // Assert 
            mockMqttConnection.VerifyAll();
        }

        [Fact]
        public async Task MessageForOtherTopicThanSubscribedToIsProcessed()
        {
            // Arrange 
            var mockMqttConnection = new Mock<IMqttConnection>();
            var mockTriggeredFunctionExecutor = new Mock<ITriggeredFunctionExecutor>();

            mockMqttConnection
                .Setup(x => x.SubscribeAsync(It.IsAny<MqttTopicFilter[]>()))
                .Returns(Task.CompletedTask);

            mockTriggeredFunctionExecutor
                .Setup(x => x.TryExecuteAsync(It.IsAny<TriggeredFunctionData>(), It.IsAny<CancellationToken>()));

            var MqttTopicFilter = new MqttTopicFilter[] { new MqttTopicFilter() { Topic = "test/topic", QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce } };
            var mqttListener = new MqttListener(mockMqttConnection.Object, MqttTopicFilter, mockTriggeredFunctionExecutor.Object, _mockLogger.Object);

            // Act
            await mqttListener.StartAsync(_cancellationToken).ConfigureAwait(false);

            var message = new MqttMessage("weird", new byte[] { }, MqttQualityOfServiceLevel.AtLeastOnce, true);
            await mqttListener.OnMessage(new MqttMessageReceivedEventArgs(message));

            // Assert 
            mockMqttConnection.VerifyAll();
            mockTriggeredFunctionExecutor.VerifyAll();
        }
    }
}
