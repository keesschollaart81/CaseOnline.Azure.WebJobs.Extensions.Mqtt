using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{
    public class MqttClientHelper : IDisposable, IMqttClientConnectedHandler, IMqttClientDisconnectedHandler, IMqttApplicationMessageReceivedHandler
    {
        static IManagedMqttClient _mqttClient;
        private static ILogger _logger;
        private readonly IManagedMqttClientOptions _options;
        private bool IsConnected { get; set; }

        private Queue<MqttApplicationMessage> MessagesReceived = new Queue<MqttApplicationMessage>();

        public static async Task<MqttClientHelper> Get(ILogger logger, int port = 1883)
        {
            _logger = logger;

            var defaultClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithTcpServer("127.0.0.1", port)
                    .WithClientId("IntegrationTest")
                    .Build())
                .Build();

            return await Get( defaultClientOptions);
        }

        public static async Task<MqttClientHelper> Get(IManagedMqttClientOptions clientOptions)
        {
            var clientHelper = new MqttClientHelper(clientOptions);
            await clientHelper.StartMqttClient();

            // wait for 5 seconds for client to be connected
            for (var i = 0; i < 100; i++)
            {
                if (clientHelper.IsConnected)
                {
                    _logger.LogDebug($"Waited for {i * 50} milliseconds for client to be connected");
                    return clientHelper;
                }
                await Task.Delay(50);
            }
            throw new Exception("Could not connect to server");
        }

        private MqttClientHelper(IManagedMqttClientOptions options)
        {
            _options = options;
        }

        private async Task StartMqttClient()
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateManagedMqttClient();
            _mqttClient.ConnectedHandler = this;
            _mqttClient.DisconnectedHandler = this;
            _mqttClient.ApplicationMessageReceivedHandler = this;

            await _mqttClient.StartAsync(_options);
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            MessagesReceived.Enqueue(eventArgs.ApplicationMessage);

            return Task.CompletedTask;
        }

        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            _logger.LogDebug($"_mqttClient_Disconnected: {eventArgs.Exception?.Message}");
            return Task.CompletedTask;
        }

        public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            IsConnected = true;
            _logger.LogDebug($"_mqttClient_Connected");
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _mqttClient.StopAsync().Wait();
            _mqttClient = null;
        }

        public async Task SubscribeAsync(string topic)
        { 
            await _mqttClient.SubscribeAsync(new List<MqttTopicFilter>() { new MqttTopicFilter() { Topic = topic, QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce } });

            await Task.Delay(TimeSpan.FromSeconds(1));
        } 

        public async Task<MqttApplicationMessage> WaitForMessage (int seconds = 10)
        {
            var totalMilliseconds = TimeSpan.FromSeconds(seconds).TotalMilliseconds;
            var sleepDuration = TimeSpan.FromMilliseconds(50); // not long otherwise MQTT Connections are being dropped?!

            for (var i = 0; i < (totalMilliseconds / sleepDuration.TotalMilliseconds); i++)
            {
                var hasMessage = MessagesReceived.TryDequeue(out var message);
                if (hasMessage)
                {
                    Debug.WriteLine($"Waited for {i * sleepDuration.TotalMilliseconds}ms");
                    return message;
                }
                await Task.Delay(sleepDuration);
            }
            return null;
        }
    }
}
