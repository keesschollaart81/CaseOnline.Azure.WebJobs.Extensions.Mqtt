using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    public class MqttConnection : IDisposable, IMqttConnection
    {
        private IMqttClientFactory _mqttClientFactory;
        private MqttConfiguration _config;
        private ILogger _logger;
        private IManagedMqttClient _managedMqttClient;

        public MqttConnection(IMqttClientFactory mqttClientFactory, MqttConfiguration config, ILogger logger)
        {
            _mqttClientFactory = mqttClientFactory;
            _config = config;
            _logger = logger;
        }

        public event Func<MqttMessageReceivedEventArgs, Task> OnMessageEventHandler;

        /// <summary>
        /// Gets a value indicating whether the listener is connected to the MQTT queue.
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Gets the descriptor for this listener.
        /// </summary> ;
        public override string ToString()
        {
            return $"Connection for config: {_config.ToString()}, connected: {Connected}";
        }

        public async Task StartAsync()
        {
            if (_managedMqttClient != null)
            {
                return;
            }

            try
            {
                _managedMqttClient = _mqttClientFactory.CreateManagedMqttClient();
                _managedMqttClient.ApplicationMessageReceived += ManagedMqttClientApplicationMessageReceived;
                _managedMqttClient.Connected += ManagedMqttClientConnected;
                _managedMqttClient.Disconnected += ManagedMqttClientDisconnected;

                await _managedMqttClient.StartAsync(_config.Options).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Unhandled exception while settingup the mqttclient to {descriptor}", e);
                throw new MqttListenerInitializationException("Unhandled exception while connectin to {descriptor}", e);
            }
        }

        private void ManagedMqttClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            Connected = false;
            _logger.LogWarning($"MqttListener Disconnected, previous connectivity state '{e.ClientWasConnected}' for {this}, message: {e.Exception?.Message}", e.Exception);
        }

        private void ManagedMqttClientConnected(object sender, MqttClientConnectedEventArgs e)
        {
            Connected = true;
            _logger.LogInformation($"MqttListener Connected, IsSessionPresent: '{e.IsSessionPresent}' for {this}");
        }

        private void ManagedMqttClientApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs mqttApplicationMessageReceivedEventArgs)
        {
            _logger.LogDebug($"MqttListener receiving message for {this}");

            var qos = (MqttQualityOfServiceLevel)Enum.Parse(typeof(MqttQualityOfServiceLevel), mqttApplicationMessageReceivedEventArgs.ApplicationMessage.QualityOfServiceLevel.ToString());

            var mqttMessage = new MqttMessage(
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Topic,
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Payload,
                qos,
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Retain);

            OnMessageEventHandler(new MqttMessageReceivedEventArgs(mqttMessage)).Wait();
        } 

        public async Task SubscribeAsync(TopicFilter[] topics)
        {
            try
            {
                await _managedMqttClient.SubscribeAsync(topics).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Unhandled exception while settingup the mqttclient to {descriptor}", e);
                throw new MqttListenerInitializationException("Unhandled exception while connectin to {descriptor}", e);
            }
        }

        public async Task UnubscribeAsync(string[] topics)
        {
            try
            {
                await _managedMqttClient.UnsubscribeAsync(topics).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Unhandled exception while settingup the mqttclient to {descriptor}", e);
                throw new MqttListenerInitializationException("Unhandled exception while connectin to {descriptor}", e);
            }
        }

        public async Task PublishAsync(MqttApplicationMessage message)
        {
            try
            {
                await _managedMqttClient.PublishAsync(message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Unhandled exception while settingup the mqttclient to {descriptor}", e);
                throw new MqttListenerInitializationException("Unhandled exception while connectin to {descriptor}", e);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _managedMqttClient.StopAsync().Wait();
            _managedMqttClient.Dispose();
            _managedMqttClient = null;

            return Task.FromResult(true);
        }

        public void Dispose()
        {
            if (_managedMqttClient != null)
            {
                _managedMqttClient.StopAsync().Wait();
                _managedMqttClient.Dispose();
                _managedMqttClient = null;
            }
        }

    }
}
