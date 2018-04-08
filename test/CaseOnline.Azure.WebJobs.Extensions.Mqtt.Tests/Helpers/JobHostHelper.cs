using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{ 
    public class JobHostHelper<T> : IDisposable
    {
        private JobHost _jobHost;

        private JobHostHelper(JobHost jobHost)
        {
            _jobHost = jobHost;
        }

        public static async Task<JobHostHelper<T>> RunFor(ILoggerFactory loggerFactory)
        {
            var config = new JobHostConfiguration();
            config.TypeLocator = new ExplicitTypeLocator(typeof(T));
            config.LoggerFactory = loggerFactory;
            config.UseDevelopmentSettings();

            config.UseMqtt();

            var jobHost = new JobHost(config);
            try
            {
                await jobHost.StartAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("In order to be able to run this integration tests, make sure you have the Azure Storage Emulator running or configure a connection to a real Azure Storage Account in the appsettings.json", ex);
            }

            var totalMilliseconds = TimeSpan.FromSeconds(5).TotalMilliseconds;
            var sleepDuration = TimeSpan.FromMilliseconds(50); // not long otherwise MQTT Connections are being dropped?!

            for (var i = 0; i < (totalMilliseconds / sleepDuration.TotalMilliseconds); i++)
            {
                if (MqttExtensionConfigProvider.MqttConnectionFactory.AllConnectionsConnected())
                {
                    Debug.WriteLine($"JobHost boot, waited for {i * sleepDuration.TotalMilliseconds}ms to be connected (MqttListener.Connected)");
                    await Task.Delay(TimeSpan.FromSeconds(1)); //wait for another second to realy become connected
                    break;
                }
                await Task.Delay(sleepDuration);
            }

            return new JobHostHelper<T>(jobHost);
        }

        public async Task CallAsync(string methodName, object arguments)
        {
            var method = typeof(T).GetMethod(methodName);
            await _jobHost.CallAsync(method, arguments);
        }

        public async Task CallAsync(string methodName)
        {
            var method = typeof(T).GetMethod(methodName);
            await _jobHost.CallAsync(methodName);
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

        public void Dispose()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1)); // let the functions finish

            _jobHost.Stop();
            _jobHost.Dispose();
        }
    }
}
