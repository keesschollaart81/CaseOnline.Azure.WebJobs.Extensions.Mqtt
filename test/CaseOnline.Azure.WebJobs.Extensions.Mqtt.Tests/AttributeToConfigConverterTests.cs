using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Util.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet.Extensions.ManagedClient;
using System;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class MqttConfigurationParserTests
    {
        private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
        private readonly MockNameResolver _resolver = new MockNameResolver();

        [Fact]
        public void ValidConfigurationIsMappedCorrect()
        {
            // Arrange 
            var mqttTriggerAttribute = new MqttTriggerAttribute("testTopic")
            {
                ConnectionString = "Server=ServerName;Port=1883;Username=UserName;Password=Password;ClientId=TestClientId"
            };

            var mqttConfigurationParser = new MqttConfigurationParser(_resolver, _mockLogger.Object);

            // Act
            var result = mqttConfigurationParser.Parse(mqttTriggerAttribute);

            // Assert  
            Assert.Equal("TestClientId", result.Options.ClientOptions.ClientId);
        }

        [Fact]
        public void InvalidPortThrowsException()
        {
            // Arrange 
            var mqttTriggerAttribute = new MqttTriggerAttribute("testTopic")
            {
                ConnectionString = "Server=ServerName;Port=ByeWorld;Username=UserName;Password=Password"
            };

            var mqttConfigurationParser = new MqttConfigurationParser(_resolver, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<FormatException>(() => mqttConfigurationParser.Parse(mqttTriggerAttribute));
        }

        [Fact]
        public void NoClientIdGuidBasedClientIdIsGenerated()
        {
            // Arrange 
            var mqttTriggerAttribute = new MqttTriggerAttribute(new[] { "testTopic" })
            {
                ConnectionString = "Server=ServerName;Port=1883;Username=UserName;Password=Password;ClientId="
            };

            var mqttConfigurationParser = new MqttConfigurationParser(_resolver, _mockLogger.Object);

            // Act 
            var result = mqttConfigurationParser.Parse(mqttTriggerAttribute);

            // Assert
            Assert.NotNull(result.Options.ClientOptions.ClientId);
            Assert.True(Guid.TryParse(result.Options.ClientOptions.ClientId, out var guid));
        }

        [Fact]
        public void NoServernameProvidedResultsInException()
        {
            // Arrange 
            var mqttTriggerAttribute = new MqttTriggerAttribute(new[] { "testTopic" })
            {
                ConnectionString = "Server=;Port=1883;Username=UserName;Password=Password;ClientId="
            };

            var mqttConfigurationParser = new MqttConfigurationParser(_resolver, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<Exception>(() => mqttConfigurationParser.Parse(mqttTriggerAttribute));

        }

        [Fact]
        public void CustomConfigProviderIsInvoked()
        {
            // Arrange  
            var mqttTriggerAttribute = new MqttTriggerAttribute(typeof(TestMqttConfigProvider));

            var mqttConfigurationParser = new MqttConfigurationParser(_resolver, _mockLogger.Object);

            // Act 
            var result = mqttConfigurationParser.Parse(mqttTriggerAttribute);

            // Assert
            Assert.NotNull(result);
        }

        private class TestMqttConfigProvider : ICreateMqttConfig
        {
            public CustomMqttConfig Create(INameResolver nameResolver, ILogger logger)
            {
                return new TestMqttConfig("", new ManagedMqttClientOptions());
            }
        }

        private class TestMqttConfig : CustomMqttConfig
        {
            public override IManagedMqttClientOptions Options { get; }

            public override string Name { get; }

            public TestMqttConfig(string name, IManagedMqttClientOptions options)
            {
                Name = name;
                Options = options;
            }
        }

        [Fact]
        public void InvalidCustomConfigCreatorThrowsException()
        {
            // Arrange  
            var mqttTriggerAttribute = new MqttTriggerAttribute(typeof(string));

            var mqttConfigurationParser = new MqttConfigurationParser(_resolver, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidCustomConfigCreatorException>(() => mqttConfigurationParser.Parse(mqttTriggerAttribute));
        }

        [Fact]
        public void BrokenCustomConfigCreatorThrowsException()
        {
            // Arrange  
            var mqttTriggerAttribute = new MqttTriggerAttribute(typeof(BrokenTestMqttConfigProvider));

            var mqttConfigurationParser = new MqttConfigurationParser(_resolver, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidCustomConfigCreatorException>(() => mqttConfigurationParser.Parse(mqttTriggerAttribute));
        }

        private class BrokenTestMqttConfigProvider : ICreateMqttConfig
        {
            public CustomMqttConfig Create(INameResolver nameResolver, ILogger logger)
            {
                throw new NotImplementedException();
            }
        }
    }
}
