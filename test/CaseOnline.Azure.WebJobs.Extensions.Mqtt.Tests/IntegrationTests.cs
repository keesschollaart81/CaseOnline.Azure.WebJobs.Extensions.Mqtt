using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Xunit;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class IntegrationTests : IDisposable
    {
        static IMqttServer _mqttServer;

        public IntegrationTests()
        {
            _mqttServer = new MqttFactory().CreateMqttServer();
            var options = new MqttServerOptions();
            options.ConnectionValidator = c =>
            {

                c.ReturnCode = MqttConnectReturnCode.ConnectionAccepted;
            };
            _mqttServer.StartAsync(options);

        }

        private void _mqttServer_Started(object sender, MqttServerStartedEventArgs e)
        {
            Debug.Write(e);
        }

        public void Dispose()
        {
            _mqttServer.StopAsync();
        }

        [Fact]
        public async Task StartAsyncSubscribesToTopics()
        {
            var locator = new ExplicitTypeLocator(typeof(TestFunctions));
            var config = new JobHostConfiguration
            {
                TypeLocator = locator,
                StorageConnectionString = "UseDevelopmentStorage=true",
                DashboardConnectionString = "UseDevelopmentStorage=true"
            };
            ILoggerFactory loggerFactory = new LoggerFactory();
            TestLoggerProvider provider = new TestLoggerProvider();
            loggerFactory.AddProvider(provider);

            config.LoggerFactory = loggerFactory;
            config.UseDevelopmentSettings();
            config.UseMqtt();
            var host = new JobHost(config);
            await host.StartAsync();
            var message = new MqttApplicationMessageBuilder().WithTopic("test/topic").WithPayload("{ \"test\":\"case\" }").Build();
            await _mqttServer.PublishAsync(message);
            for (var i = 0; i < 50; i++)
            {
                await Task.Delay(200);
            }
            await host.StopAsync();
            var messages = provider.GetAllLogMessages();
            foreach (var message2 in messages)
            {
                Debug.WriteLine(message2.FormattedMessage);
            }
        }
    }

    public class ExplicitTypeLocator : ITypeLocator
    {
        private readonly IReadOnlyList<Type> types;

        public ExplicitTypeLocator(params Type[] types)
        {
            this.types = types.ToList().AsReadOnly();
        }

        public IReadOnlyList<Type> GetTypes()
        {
            return types;
        }
    }

    public static class TestFunctions
    {
        public static void Testert([MqttTrigger(new[] { "test/topic" })] PublishedMqttMessage timer)
        {
        }

    }

    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly Regex userCategoryRegex = new Regex(@"^Function\.\w+\.User$");

        public IList<TestLogger> CreatedLoggers = new List<TestLogger>();

        public TestLoggerProvider(Func<string, LogLevel, bool> filter = null)
        {
            _filter = filter ?? new LogCategoryFilter().Filter;
        }

        public ILogger CreateLogger(string categoryName)
        {
            var logger = new TestLogger(categoryName, _filter);
            CreatedLoggers.Add(logger);
            return logger;
        }

        public IEnumerable<LogMessage> GetAllLogMessages()
        {
            return CreatedLoggers.SelectMany(l => l.LogMessages);
        }

        public IEnumerable<LogMessage> GetAllUserLogMessages()
        {
            return GetAllLogMessages().Where(p => userCategoryRegex.IsMatch(p.Category));
        }

        public void Dispose()
        {
        }
    }
    public class TestLogger : ILogger
    {
        private readonly Func<string, LogLevel, bool> _filter;

        public TestLogger(string category, Func<string, LogLevel, bool> filter = null)
        {
            Category = category;
            _filter = filter;
        }

        public string Category { get; private set; }

        public IList<LogMessage> LogMessages { get; } = new List<LogMessage>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter?.Invoke(Category, logLevel) ?? true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            LogMessages.Add(new LogMessage
            {
                Level = logLevel,
                EventId = eventId,
                State = state as IEnumerable<KeyValuePair<string, object>>,
                Exception = exception,
                FormattedMessage = formatter(state, exception),
                Category = Category
            });
        }
    }

    public class LogMessage
    {
        public LogLevel Level { get; set; }

        public EventId EventId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> State { get; set; }

        public Exception Exception { get; set; }

        public string FormattedMessage { get; set; }

        public string Category { get; set; }
    }
}
