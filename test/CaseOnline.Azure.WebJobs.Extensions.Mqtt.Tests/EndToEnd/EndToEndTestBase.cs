using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers.Logging;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.EndToEnd
{
    public abstract class EndToEndTestBase
    {
        protected ILogger _logger;
        protected ILoggerFactory _loggerFactory;

        protected MqttApplicationMessage DefaultMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test/topic")
                    .WithPayload("{ \"test\":\"case\" }")
                    .WithAtLeastOnceQoS()
                    .Build();

        public EndToEndTestBase()
        {
            var filterOptions = new LoggerFilterOptions
            {
                MinLevel = LogLevel.Trace
            };
            _loggerFactory = new LoggerFactory(new[] { new TestLoggerProvider() }, filterOptions);
            //_loggerFactory.AddProvider();
            _logger = _loggerFactory.CreateLogger("EndToEndTests");
        }

        public async Task WaitFor(Func<bool> condition, int seconds = 10)
        {
            await WaitFor(condition, TimeSpan.FromSeconds(seconds));
        }

        public async Task WaitFor(Func<bool> condition, TimeSpan timeout)
        {
            var totalMilliseconds = timeout.TotalMilliseconds;
            var sleepDuration = TimeSpan.FromMilliseconds(50); // not long otherwise MQTT Connections are being dropped?!

            for (var i = 0; i < (totalMilliseconds / sleepDuration.TotalMilliseconds); i++)
            {
                if (condition())
                {
                    Debug.WriteLine($"Waited for {i * sleepDuration.TotalMilliseconds}ms");
                    break;
                }
                await Task.Delay(sleepDuration);
            }
            await Task.Delay(1000); // after the condition is met, wait a bit for the function to finish
        }
    }
}
