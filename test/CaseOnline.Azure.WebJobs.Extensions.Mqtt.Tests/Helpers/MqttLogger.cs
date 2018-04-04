using System;
using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{
    public class MqttLogger : IMqttNetLogger
    {
        public MqttLogger(ILogger logger)
        {
            Logger = logger;
            LogMessagePublished += MqttLogger_LogMessagePublished;
        }

        public ILogger Logger { get; }

#pragma warning disable 67
        public event EventHandler<MqttNetLogMessagePublishedEventArgs> LogMessagePublished;
#pragma warning restore 67

        private void MqttLogger_LogMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
        {
            Logger.LogTrace($"{e.TraceMessage.Level}:{e.TraceMessage.Message}");
        }

        public void Error<TSource>(Exception exception, string message, params object[] parameters)
        {
            Logger.LogError(exception, $"{message},{string.Join(",", parameters)}");
        }

        public void Error<TSource>(string message, params object[] parameters)
        {
            Logger.LogError($"{message},{string.Join(",", parameters)}");
        }

        public void Info<TSource>(string message, params object[] parameters)
        {
            Logger.LogInformation(message);
        }

        public void Trace<TSource>(string message, params object[] parameters)
        {
            Logger.LogTrace(message);
        }

        public void Warning<TSource>(Exception exception, string message, params object[] parameters)
        {
            Logger.LogWarning(message);
        }

        public void Warning<TSource>(string message, params object[] parameters)
        {
            Logger.LogWarning(message);
        }
    }
}
