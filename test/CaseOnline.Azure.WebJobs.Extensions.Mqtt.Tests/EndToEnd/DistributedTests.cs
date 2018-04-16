using System.Text;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers;
using MQTTnet;
using MQTTnet.Server;
using Xunit;
using System.Collections.Async;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.EndToEnd
{
    public class DistributedTests : EndToEndTestBase
    {
        [Fact(Skip = "Mqtt Testserver connection drops")]
        public async Task MultipleJobHostsOnlyOneIsActive()
        {
            var clientIdsSeen = new List<string>();
            var options = new MqttServerOptionsBuilder()
                .WithConnectionValidator((x) =>
                {
                    if (x.ClientId != "IntegrationTest" && !clientIdsSeen.Any(y => y == x.ClientId))
                    {
                        clientIdsSeen.Add(x.ClientId);
                    }
                })
                .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger, options))
            using (var jobHost1 = await JobHostHelper<MultipleJobHostsTestFunction>.RunFor(_loggerFactory, true))
            using (var jobHost2 = await JobHostHelper<MultipleJobHostsTestFunction>.RunFor(_loggerFactory, false))
            {
                for (int i = 0; i < 30; i++)
                {
                    await mqttServer.PublishAsync(DefaultMessage);
                }
                await WaitFor(() => MultipleJobHostsTestFunction.CallCount >= 30);
            }

            Assert.Equal(30, MultipleJobHostsTestFunction.CallCount);
            Assert.Single(clientIdsSeen);
            Assert.Equal("test/topic", MultipleJobHostsTestFunction.LastReceivedMessage.Topic);
            var messageBody = Encoding.UTF8.GetString(MultipleJobHostsTestFunction.LastReceivedMessage.GetMessage());
            Assert.Equal("{ \"test\":\"case\" }", messageBody);
        }

        private class MultipleJobHostsTestFunction
        {
            public static int CallCount = 0;
            public static IMqttMessage LastReceivedMessage;

            public static void Testert([MqttTrigger("test/topic")] IMqttMessage mqttMessage)
            {
                Interlocked.Increment(ref CallCount);
                LastReceivedMessage = mqttMessage;
            }
        }
    }
}
