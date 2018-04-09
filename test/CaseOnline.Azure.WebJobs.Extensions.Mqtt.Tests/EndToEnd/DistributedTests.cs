using System.Text;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers;
using MQTTnet;
using MQTTnet.Server;
using Xunit;
using System.Collections.Async;
using System.Linq;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.EndToEnd
{
    public class DistributedTests : EndToEndTestBase
    {
        [Fact]
        public async Task MultipleJobHostsOnlyOneIsActive()
        {
            var totalConnections = 0; 
            var options = new MqttServerOptionsBuilder()
                .WithConnectionValidator(x => totalConnections += (x.ClientId != "IntegrationTest") ? 1 : 0)
                .Build();

            using (var mqttServer = await MqttServerHelper.Get(_logger, options))
            using (var jobHost1 = await JobHostHelper<MultipleJobHostsTestFunction>.RunFor(_loggerFactory))
            using (var jobHost2 = await JobHostHelper<MultipleJobHostsTestFunction>.RunFor(_loggerFactory))
            {
                await Enumerable.Range(0, 30).ParallelForEachAsync(async (y) =>
                 {
                     await mqttServer.PublishAsync(DefaultMessage);
                 }, 30);

                await WaitFor(() => MultipleJobHostsTestFunction.CallCount >= 30);
            }

            Assert.Equal(1, totalConnections);
            Assert.Equal(30, MultipleJobHostsTestFunction.CallCount);
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
                CallCount++;
                LastReceivedMessage = mqttMessage;
            }
        }
    }
}
