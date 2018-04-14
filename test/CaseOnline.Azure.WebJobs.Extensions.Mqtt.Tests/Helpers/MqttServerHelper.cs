using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Implementations;
using MQTTnet.Server;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{
    public class MqttServerHelper : IDisposable, IApplicationMessagePublisher
    {
        private IMqttServer _mqttServer;
        private readonly ILogger _logger;
        private readonly IMqttServerOptions _options;
        public event EventHandler<OnMessageEventArgs> OnMessage;
        private bool serverStarted = false;

        public static async Task<MqttServerHelper> Get(ILogger logger)
        {
            var defaultServerOptions = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(1883)
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
            MqttTcpChannel.CustomCertificateValidationCallback = RemoteValidation;
            var logger = new MqttLogger(_logger);
            var factory = new MqttFactory();
            _mqttServer = factory.CreateMqttServer(new List<IMqttServerAdapter> { new MqttServerAdapter(logger) }, logger);
            _mqttServer.Started += Started;
            _mqttServer.ClientConnected += ClientConnected;
            _mqttServer.ClientDisconnected += ClientDisconnected;

            await _mqttServer.StartAsync(_options);

            // wait for 5 seconds for server to be started
            for(var i = 0; i < 100; i++)
            {
                if (serverStarted)
                {
                    return;
                }
                await Task.Delay(50);
            }
            throw new Exception("Mqtt Server did not start?");
        }

        private void ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            OnMessage(this, new OnMessageEventArgs(e.ClientId, e.ApplicationMessage));
        }

        private bool RemoteValidation(X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors, MqttClientTcpOptions options)
        {
            _logger.LogDebug($"RemoteValidation: {sslPolicyErrors}");
            return true;
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
            serverStarted = true;
            _logger.LogDebug($"mqtt server started: {e}");
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
            var clients = await _mqttServer.GetConnectedClientsAsync();
            await _mqttServer.SubscribeAsync("Custom", new List<TopicFilter>() { new TopicFilter(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) });
        }
    }
}
