using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers.Logging
{
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

            Debug.WriteLine($"{DateTime.Now:hh:mm:ss.fff}: {formatter(state, exception)}");

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
}
