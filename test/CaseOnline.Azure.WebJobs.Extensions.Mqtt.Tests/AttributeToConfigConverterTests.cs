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
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using System.Linq;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Util;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class AttributeToConfigConverterTests
    {
        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();

        [Fact]
        public async Task ConfiguredTopicsAreMappedCorrect()
        {
            // Arrange 
            var mqttTriggerAttribute = new MqttTriggerAttribute(new[] { "testTopic" })
            {
                ServerName = "ServerName",
                PortName = "1883",
                UsernameName = "UserName",
                PasswordName = "Password"
            };

            var resolver = new MockNameResolver();
            var attributeToConfigConverter = new AttributeToConfigConverter(mqttTriggerAttribute, resolver, _mockLogger.Object);

            // Act
            var result = attributeToConfigConverter.GetMqttConfiguration();

            // Assert  
            Assert.Equal(mqttTriggerAttribute.Topics, result.Topics.Select(x => x.Topic));
        }
    }
}
