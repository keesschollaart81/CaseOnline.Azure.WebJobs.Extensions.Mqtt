using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client.Publishing;
using MQTTnet.Implementations;
using MQTTnet.Server;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{
    public class MqttServerHelper : IApplicationMessagePublisher, IDisposable, IMqttServerStartedHandler, IMqttServerClientConnectedHandler, IMqttServerClientDisconnectedHandler
    {
        private IMqttServer _mqttServer;
        private readonly ILogger _logger;
        private readonly IMqttServerOptions _options; 
        private bool serverStarted = false;

        public static async Task<MqttServerHelper> Get(ILogger logger, int port = 1883)
        {
            var defaultServerOptions = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(port)
                .Build();

            return await Get(logger, defaultServerOptions);
        }

        public static async Task<MqttServerHelper> Get(ILogger logger, IMqttServerOptions serverOptions)
        {
            var serverHelper = new MqttServerHelper(logger, serverOptions);
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
            var logger = new MqttLogger(_logger);
            var factory = new MqttFactory();
            _mqttServer = factory.CreateMqttServer(new List<IMqttServerAdapter> { new MqttTcpServerAdapter(logger.CreateChildLogger()) }, logger);
            _mqttServer.StartedHandler = this;
            _mqttServer.ClientConnectedHandler = this;
            _mqttServer.ClientDisconnectedHandler = this;

            await _mqttServer.StartAsync(_options);

            // wait for 5 seconds for server to be started
            for (var i = 0; i < 100; i++)
            {
                if (serverStarted)
                {
                    _logger.LogDebug($"Waited for {i * 50} milliseconds for server to be started");
                    return;
                }
                await Task.Delay(50);
            }
            throw new Exception("Mqtt Server did not start?");
        }
         
        public Task HandleClientDisconnectedAsync(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            _logger.LogDebug($"_mqttServer_ClientDisconnected: {eventArgs.ClientId}");
            return Task.CompletedTask;
        }

        public Task HandleClientConnectedAsync(MqttServerClientConnectedEventArgs eventArgs)
        {
            _logger.LogDebug($"_mqttServer_ClientConnected: {eventArgs.ClientId}");
            return Task.CompletedTask;
        }


        public Task HandleServerStartedAsync(EventArgs eventArgs)
        {
            serverStarted = true;
            _logger.LogDebug($"mqtt server started: {eventArgs}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            serverStarted = false;
            _mqttServer.StopAsync().Wait();
            _mqttServer = null;
        }

        public async Task PublishAsync(IEnumerable<MqttApplicationMessage> applicationMessages)
        {
            await _mqttServer.PublishAsync(applicationMessages);
        }

        public async Task SubscribeAsync(string topic)
        {
            await _mqttServer.SubscribeAsync("Custom", new List<MqttTopicFilter>() { new MqttTopicFilter() { Topic = topic, QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce } });
        }

        public async Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage)
        {
            return await PublishAsync(applicationMessage, CancellationToken.None);
        }

        public async Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken)
        {
            return await _mqttServer.PublishAsync(applicationMessage);
        }
    }
}
