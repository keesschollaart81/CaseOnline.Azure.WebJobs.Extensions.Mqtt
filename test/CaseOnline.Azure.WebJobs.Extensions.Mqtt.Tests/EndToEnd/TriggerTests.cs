using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers;
using Microsoft.Azure.WebJobs.Host.Indexers;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.EndToEnd
{
    public class TriggerTests : EndToEndTestBase
    {
        [Fact]
        public async Task SimpleMessageIsReceived()
        {
            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<SimpleMessageIsReceivedTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await WaitFor(() => SimpleMessageIsReceivedTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, SimpleMessageIsReceivedTestFunction.CallCount);
            Assert.Equal("test/topic", SimpleMessageIsReceivedTestFunction.LastReceivedMessage.Topic);
            var messageBody = Encoding.UTF8.GetString(SimpleMessageIsReceivedTestFunction.LastReceivedMessage.GetMessage());
            Assert.Equal("{ \"test\":\"case\" }", messageBody);
        }

        private class SimpleMessageIsReceivedTestFunction
        {
            public static int CallCount = 0;
            public static IMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic")] IMqttMessage mqttMessage)
            {
                CallCount++;
                LastReceivedMessage = mqttMessage;
            }
        }

        [Fact]
        public async Task CustomConnectionWithClientIdIsReceived()
        {
            string clientId = string.Empty;
            var options = new MqttServerOptionsBuilder()
                   .WithConnectionValidator(x => clientId = x.ClientId)
                   .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger, options))
            using (var jobHost = await JobHostHelper<CustomConnectionStringWithClientIdTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await WaitFor(() => CustomConnectionStringWithClientIdTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, CustomConnectionStringWithClientIdTestFunction.CallCount);
            Assert.Equal("Custom", clientId);
        }

        private class CustomConnectionStringWithClientIdTestFunction
        {
            public static int CallCount = 0;
            public static IMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic", ConnectionString = "MqttConnectionWithCustomClientId")] IMqttMessage mqttMessage)
            {
                CallCount++;
                LastReceivedMessage = mqttMessage;
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
                    x.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    if (x.Username == "admin" && x.Password == "Welkom123")
                    {
                        x.ReasonCode = MqttConnectReasonCode.Success;
                    }
                })
                .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger, options))
            using (var jobHost = await JobHostHelper<UsernameAndPasswordTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await WaitFor(() => UsernameAndPasswordTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, UsernameAndPasswordTestFunction.CallCount);
            Assert.True(validated, "Username and Password are not validated by the Mqtt server");
        }

        private class UsernameAndPasswordTestFunction
        {
            public static int CallCount = 0;

            public static void Testert([MqttTrigger("test/topic", ConnectionString = "MqttConnectionWithUsernameAndPassword")] IMqttMessage mqttMessage)
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
            using (var jobHost = await JobHostHelper<SimpleMessageAnotherPortTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await WaitFor(() => SimpleMessageAnotherPortTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, SimpleMessageAnotherPortTestFunction.CallCount);
        }

        private class SimpleMessageAnotherPortTestFunction
        {
            public static int CallCount = 0;
            public static IMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic", ConnectionString = "MqttConnectionWithCustomPort")] IMqttMessage mqttMessage)
            {
                CallCount++;
                LastReceivedMessage = mqttMessage;
            }
        }

        [Fact]
        public async Task WhenTlsIsSetToTrueASecureConnectionIsMade()
        {

            var options = new MqttServerOptionsBuilder()
               .WithEncryptedEndpoint()
               .WithEncryptionCertificate(new X509Certificate2(@"Certificates/myRootCA.pfx", "12345", X509KeyStorageFlags.Exportable))
               .WithoutDefaultEndpoint()
               .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger, options))
            using (var jobHost = await JobHostHelper<FunctionConnectingWithTlsEnabledTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await WaitFor(() => FunctionConnectingWithTlsEnabledTestFunction.CallCount >= 1, 20);
            }

            Assert.Equal(1, FunctionConnectingWithTlsEnabledTestFunction.CallCount);
        }

        private class FunctionConnectingWithTlsEnabledTestFunction
        {
            public static int CallCount = 0;
            public static IMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic", ConnectionString = "MqttConnectionWithTls")] IMqttMessage mqttMessage)
            {
                CallCount++;
                LastReceivedMessage = mqttMessage;
            }
        }

        [Fact]
        public async Task MultipleTriggersWithSameConnectionThrowsExceptiojn()
        {
            using (var mqttServer = await MqttServerHelper.Get(_logger))
            {
                JobHostHelper<MultipleTriggersSameConnectionTestFunction> jobHostHelper = null;
                try
                {
                    var ex = await Assert.ThrowsAsync<FunctionIndexingException>(async () => jobHostHelper = await JobHostHelper<MultipleTriggersSameConnectionTestFunction>.RunFor(_testLoggerProvider));

                }
                finally
                {
                    jobHostHelper?.Dispose();
                }
            }
        }

        private class MultipleTriggersSameConnectionTestFunction
        {
            public static int CallCount = 0;
            public static IMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic")] IMqttMessage mqttMessage)
            {
                CallCount++;
                LastReceivedMessage = mqttMessage;
            }

            public static void Testert2([MqttTrigger("test/topic2")] IMqttMessage mqttMessage)
            {
                CallCount++;
                LastReceivedMessage = mqttMessage;
            }
        }

        [Fact]
        public async Task MultipleTriggersReceiveOwnMessages()
        {
            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<MultipleTriggersTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                var secondMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test/topic2")
                    .WithPayload("{ \"test\":\"case\" }")
                    .WithAtLeastOnceQoS()
                    .Build();
                await mqttServer.PublishAsync(secondMessage);

                await WaitFor(() => MultipleTriggersTestFunction.CallCountFunction1 >= 1 && MultipleTriggersTestFunction.CallCountFunction2 >= 1);
            }

            Assert.Equal(1, MultipleTriggersTestFunction.CallCountFunction1);
            Assert.Equal(1, MultipleTriggersTestFunction.CallCountFunction2);
            Assert.Equal("test/topic", MultipleTriggersTestFunction.LastReceivedMessageFunction1.Topic);
            Assert.Equal("test/topic2", MultipleTriggersTestFunction.LastReceivedMessageFunction2.Topic);
        }

        private class MultipleTriggersTestFunction
        {
            public static int CallCountFunction1 = 0;
            public static int CallCountFunction2 = 0;
            public static IMqttMessage LastReceivedMessageFunction1;
            public static IMqttMessage LastReceivedMessageFunction2;

            public static void Testert([MqttTrigger("test/topic")] IMqttMessage mqttMessage)
            {
                CallCountFunction1++;
                LastReceivedMessageFunction1 = mqttMessage;
            }

            public static void Testert2([MqttTrigger("test/topic2", ConnectionString = "MqttConnectionWithCustomClientId")] IMqttMessage mqttMessage)
            {
                CallCountFunction2++;
                LastReceivedMessageFunction2 = mqttMessage;
            }
        }

        [Fact]
        public async Task MultipleTriggersCustomConfigReceiveOwnMessages()
        {
            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<MultipleTriggersCustomConfigTestFunction>.RunFor(_testLoggerProvider))
            {
                var firstMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test/asd/1")
                    .WithPayload("{ \"test\":\"case\" }")
                    .WithAtLeastOnceQoS()
                    .Build();
                await mqttServer.PublishAsync(firstMessage);

                var secondMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test/asd/2")
                    .WithPayload("{ \"test\":\"case\" }")
                    .WithAtLeastOnceQoS()
                    .Build();
                await mqttServer.PublishAsync(secondMessage);

                await WaitFor(() => MultipleTriggersCustomConfigTestFunction.CallCountFunction1 >= 1 && MultipleTriggersCustomConfigTestFunction.CallCountFunction2 >= 1);
            }

            Assert.Equal(1, MultipleTriggersCustomConfigTestFunction.CallCountFunction1);
            Assert.Equal(1, MultipleTriggersCustomConfigTestFunction.CallCountFunction2);
            Assert.Equal("test/asd/1", MultipleTriggersCustomConfigTestFunction.LastReceivedMessageFunction1.Topic);
            Assert.Equal("test/asd/2", MultipleTriggersCustomConfigTestFunction.LastReceivedMessageFunction2.Topic);
        }

        private class MultipleTriggersCustomConfigTestFunction
        {
            public static int CallCountFunction1 = 0;
            public static int CallCountFunction2 = 0;
            public static IMqttMessage LastReceivedMessageFunction1;
            public static IMqttMessage LastReceivedMessageFunction2;

            public static void Testert([MqttTrigger(typeof(TestMqttConfigProvider), "test/+/1")] IMqttMessage mqttMessage)
            {
                CallCountFunction1++;
                LastReceivedMessageFunction1 = mqttMessage;
            }

            public static void Testert2([MqttTrigger(typeof(TestMqttConfigProvider), "test/+/2")] IMqttMessage mqttMessage)
            {
                CallCountFunction2++;
                LastReceivedMessageFunction2 = mqttMessage;
            }
        }

        [Fact]
        public async Task CustomMqttConfigProviderGetsTriggered()
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("TestTopic/random")
                .WithPayload("{ \"test\":\"case\" }")
                .WithAtLeastOnceQoS()
                .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<CustomMqttConfigProviderTestFunction>.RunFor(_testLoggerProvider))
            {
                await mqttServer.PublishAsync(message);

                await WaitFor(() => CustomMqttConfigProviderTestFunction.CallCount >= 1);
            }

            Assert.Equal(1, CustomMqttConfigProviderTestFunction.CallCount);
            Assert.Equal("TestTopic/random", CustomMqttConfigProviderTestFunction.LastReceivedMessage.Topic);
            var messageBody = Encoding.UTF8.GetString(CustomMqttConfigProviderTestFunction.LastReceivedMessage.GetMessage());
            Assert.Equal("{ \"test\":\"case\" }", messageBody);
        }

        private class CustomMqttConfigProviderTestFunction
        {
            public static int CallCount = 0;
            public static IMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger(typeof(TestMqttConfigProvider), "%TopicName%/#")] IMqttMessage mqttMessage)
            {
                CallCount++;
                LastReceivedMessage = mqttMessage;
            }
        }


        [Fact]
        public async Task ComplexTopicFilterIsUsed()
        {
            using (var mqttServer = await MqttServerHelper.Get(_logger))
            using (var jobHost = await JobHostHelper<ComplexTopicFilterIsUsedFunction>.RunFor(_testLoggerProvider))
            {
                await mqttServer.PublishAsync(DefaultMessage);

                await WaitFor(() => ComplexTopicFilterIsUsedFunction.CallCount >= 1);
            }

            Assert.Equal(1, ComplexTopicFilterIsUsedFunction.CallCount);
            Assert.Equal("test/topic", ComplexTopicFilterIsUsedFunction.LastReceivedMessage.Topic);
            Assert.Equal(Messaging.MqttQualityOfServiceLevel.AtMostOnce, ComplexTopicFilterIsUsedFunction.LastReceivedMessage.QosLevel);
            var messageBody = Encoding.UTF8.GetString(ComplexTopicFilterIsUsedFunction.LastReceivedMessage.GetMessage());
            Assert.Equal("{ \"test\":\"case\" }", messageBody);
        }

        private class ComplexTopicFilterIsUsedFunction
        {
            public static int CallCount = 0;
            public static IMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic", Messaging.MqttQualityOfServiceLevel.AtMostOnce, Messaging.NoLocal.False, RetainAsPublished.True, Messaging.MqttRetainHandling.SendAtSubscribe)] IMqttMessage mqttMessage)
            {
                CallCount++;
                LastReceivedMessage = mqttMessage;
            }
        }
    }
}
