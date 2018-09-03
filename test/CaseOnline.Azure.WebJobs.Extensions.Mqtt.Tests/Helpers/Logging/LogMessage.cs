using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers.Logging
{
    public class LogMessage
    {
        public LogLevel Level { get; set; }

        public EventId EventId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> State { get; set; }

        public Exception Exception { get; set; }

        public string FormattedMessage { get; set; }

        public string Category { get; set; }

        public DateTime Timestamp { get; set; }

        public override string ToString() => $"[{Timestamp.ToString("HH:mm:ss.fff")}] [{Category}] {FormattedMessage} {Exception}";
    }
}
