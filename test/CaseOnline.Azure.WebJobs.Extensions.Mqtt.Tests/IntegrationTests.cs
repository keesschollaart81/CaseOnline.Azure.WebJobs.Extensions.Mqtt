using System;
using System.Text;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers.Logging;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class IntegrationTests
    {
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;

        private MqttApplicationMessage DefaultMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test/topic")
                    .WithPayload("{ \"test\":\"case\" }")
                    .WithAtLeastOnceQoS()
                    .Build();

        public IntegrationTests()
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new TestLoggerProvider());
            _logger = _loggerFactory.CreateLogger("IntegrationTests");
        }

        [Fact]
        public async Task SimpleMessageIsReceived()
        {
            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var jobHost = await JobHostHelper.RunFor<SimpleMessageIsReceivedTestFunction>(_loggerFactory))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await jobHost.WaitFor(() => SimpleMessageIsReceivedTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, SimpleMessageIsReceivedTestFunction.CallCount);
            Assert.Equal("test/topic", SimpleMessageIsReceivedTestFunction.LastReceivedMessage.Topic);
            var messageBody = Encoding.UTF8.GetString(SimpleMessageIsReceivedTestFunction.LastReceivedMessage.GetMessage());
            Assert.Equal("{ \"test\":\"case\" }", messageBody);
        }

        private class SimpleMessageIsReceivedTestFunction
        {
            public static int CallCount = 0;
            public static PublishedMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic")] PublishedMqttMessage publishedMqttMessage)
            {
                CallCount++;
                LastReceivedMessage = publishedMqttMessage;
            }
        }

        [Fact]
        public async Task CustomConnectionStringIsReceived()
        {
            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var jobHost = await JobHostHelper.RunFor<CustomConnectionStringTestFunction>(_loggerFactory))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await jobHost.WaitFor(() => CustomConnectionStringTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, CustomConnectionStringTestFunction.CallCount);
        }

        private class CustomConnectionStringTestFunction
        {
            public static int CallCount = 0;
            public static PublishedMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic", ConnectionString = "CustomMqttConnection")] PublishedMqttMessage publishedMqttMessage)
            {
                CallCount++;
                LastReceivedMessage = publishedMqttMessage;
            }
        }

        [Fact]
        public async Task UsernameAndPasswordAreValidated()
        {
            var validated = false;
            var options = new MqttServerOptionsBuilder()
                .WithConnectionValidator(x =>
                {
                    validated = true;
                    x.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                    if (x.Username == "admin" && x.Password == "Welkom123")
                    {
                        x.ReturnCode = MqttConnectReturnCode.ConnectionAccepted;
                    }
                })
                .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger, options))
            using (var jobHost = await JobHostHelper.RunFor<UsernameAndPasswordTestFunction>(_loggerFactory))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await jobHost.WaitFor(() => UsernameAndPasswordTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, UsernameAndPasswordTestFunction.CallCount);
            Assert.True(validated, "Username and Password are not validated by the Mqtt server");
        }

        private class UsernameAndPasswordTestFunction
        {
            public static int CallCount = 0;

            public static void Testert([MqttTrigger("test/topic", ConnectionString = "MqttConnectionWithUsernameAndPassword")] PublishedMqttMessage publishedMqttMessage)
            {
                CallCount++;
            }
        }

        [Fact]
        public async Task MqttServerOnOtherPortReceivesMessage()
        {
            var options = new MqttServerOptionsBuilder()
               .WithDefaultEndpointPort(1337)
               .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger, options))
            using (var jobHost = await JobHostHelper.RunFor<SimpleMessageAnotherPortTestFunction>(_loggerFactory))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await jobHost.WaitFor(() => SimpleMessageAnotherPortTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, SimpleMessageAnotherPortTestFunction.CallCount);
        }

        private class SimpleMessageAnotherPortTestFunction
        {
            public static int CallCount = 0;
            public static PublishedMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic", ConnectionString = "MqttConnectionWithCustomPort")] PublishedMqttMessage publishedMqttMessage)
            {
                CallCount++;
                LastReceivedMessage = publishedMqttMessage;
            }
        }
    }

}
