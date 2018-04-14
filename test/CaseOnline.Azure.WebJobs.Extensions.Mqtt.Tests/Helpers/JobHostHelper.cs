using System;
using System.Diagnostics;
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

        public static async Task<JobHostHelper<T>> RunFor(ILoggerFactory loggerFactory, bool waitForAllConnectionToBeConnected = true)
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

            var jobHostHelper = new JobHostHelper<T>(jobHost);

            if (waitForAllConnectionToBeConnected)
            {
                await jobHostHelper.WaitForAllConnectionToBeConnected();
            }

            return jobHostHelper;
        }

        public async Task WaitForAllConnectionToBeConnected()
        {
            var totalMilliseconds = TimeSpan.FromSeconds(5).TotalMilliseconds;
            var sleepDuration = TimeSpan.FromMilliseconds(50); // not long otherwise MQTT Connections are being dropped?!

            var mqttExtensionConfigProvider = _jobHost.Services.GetService(typeof(IMqttConnectionFactory)) as MqttConnectionFactory;
            for (var i = 0; i < (totalMilliseconds / sleepDuration.TotalMilliseconds); i++)
            {
                if (mqttExtensionConfigProvider.AllConnectionsConnected())
                {
                    Debug.WriteLine($"JobHost boot, waited for {i * sleepDuration.TotalMilliseconds}ms to be connected");
                    await Task.Delay(sleepDuration); //wait for another second to realy become connected
                    break;
                }
                await Task.Delay(sleepDuration);
            }
            if (!mqttExtensionConfigProvider.AllConnectionsConnected())
            {
                throw new Exception("Not all connections could be connected");
            }
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

        public void Dispose()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1)); // let the functions finish

            // this should not be needed, but for some reason some connections stay open / references keep exist after JobHost instance is stopped and disposed
            var mqttExtensionConfigProvider = _jobHost.Services.GetService(typeof(IMqttConnectionFactory)) as MqttConnectionFactory;
            mqttExtensionConfigProvider.DisconnectAll().Wait();

            _jobHost.Stop();
            _jobHost.Dispose();
            _jobHost = null;
        }
    }
}
