using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{
    public class MqttClientHelper : IDisposable
    {
        static IManagedMqttClient _mqttClient;
        private readonly ILogger _logger;
        private readonly IManagedMqttClientOptions _options;
        public event EventHandler<OnMessageEventArgs> OnMessage;
        private bool IsConnected { get; set; }

        public static async Task<MqttClientHelper> Get(ILogger logger)
        {
            var defaultClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithTcpServer("localhost")
                    .WithClientId("IntegrationTest")
                    .Build())
                .Build();

            return await Get(logger, defaultClientOptions);
        }

        public static async Task<MqttClientHelper> Get(ILogger logger, IManagedMqttClientOptions clientOptions)
        {
            var clientHelper = new MqttClientHelper(logger, clientOptions);
            await clientHelper.StartMqttClient();

            // wait for 5 seconds for client to be connected
            for (var i = 0; i < 100; i++)
            {
                if (clientHelper.IsConnected)
                {
                    return clientHelper;
                }
                await Task.Delay(50);
            }
            throw new Exception("Could not connect to server");
        }

        private MqttClientHelper(ILogger logger, IManagedMqttClientOptions options)
        {
            _logger = logger;
            _options = options;
        }

        private async Task StartMqttClient()
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateManagedMqttClient();
            _mqttClient.Connected += _mqttClient_Connected;
            _mqttClient.Disconnected += _mqttClient_Disconnected;
            _mqttClient.ApplicationMessageReceived += _mqttClient_ApplicationMessageReceived;

            await _mqttClient.StartAsync(_options);
        }

        private void _mqttClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            OnMessage(this, new OnMessageEventArgs(e.ClientId, e.ApplicationMessage));
        }

        private void _mqttClient_Disconnected(object sender, MQTTnet.Client.MqttClientDisconnectedEventArgs e)
        {
            _logger.LogDebug($"_mqttClient_Disconnected: {e.Exception?.Message}");
        }

        private void _mqttClient_Connected(object sender, MQTTnet.Client.MqttClientConnectedEventArgs e)
        {
            IsConnected = true;
            _logger.LogDebug($"_mqttClient_Connected");
        }
        
        public void Dispose()
        {
            _mqttClient.StopAsync().Wait();
            _mqttClient = null;
        }

        public async Task SubscribeAsync(string topic)
        { 
            await _mqttClient.SubscribeAsync(new List<TopicFilter>() { new TopicFilter(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) });
        }
    }
}
