using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{
    public class MqttServerHelper : IDisposable, IApplicationMessagePublisher
    {
        static IMqttServer _mqttServer;
        private readonly ILogger _logger;
        private readonly IMqttServerOptions _options;

        public static async Task<MqttServerHelper> Get(ILogger logger)
        {
            var defaultOptions = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(1883)
                .Build();

            return await Get(logger, defaultOptions);
        }
        public static async Task<MqttServerHelper> Get(ILogger logger, IMqttServerOptions options)
        {
            var serverHelper = new MqttServerHelper(logger, options);
            await serverHelper.StartMqttServer();
            return serverHelper;
        }

        private MqttServerHelper(ILogger logger, IMqttServerOptions options)
        {
            _logger = logger;
            _options = options;
        }

        private async Task StartMqttServer()
        {
            _mqttServer = new MqttFactory().CreateMqttServer();
            _mqttServer.Started += Started;
            _mqttServer.ClientConnected += ClientConnected;
            _mqttServer.ClientDisconnected += ClientDisconnected;

            await _mqttServer.StartAsync(_options);
        }

        private void ClientDisconnected(object sender, MQTTnet.Server.MqttClientDisconnectedEventArgs e)
        {
            _logger.LogDebug($"_mqttServer_ClientDisconnected: {e.Client.ClientId}");
        }

        private void ClientConnected(object sender, MQTTnet.Server.MqttClientConnectedEventArgs e)
        {
            _logger.LogDebug($"_mqttServer_ClientConnected: {e.Client.ClientId}");
        }

        private void Started(object sender, MqttServerStartedEventArgs e)
        {
            _logger.LogDebug($"mqtt server started: {e}");
        }
        public void Dispose()
        {
            _mqttServer.StopAsync().Wait();
            _mqttServer = null;
        }

        public async Task PublishAsync(IEnumerable<MqttApplicationMessage> applicationMessages)
        {
            await _mqttServer.PublishAsync(applicationMessages);
        }
    }
}
