using System;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    /// <summary>
    /// Manages the state of the MQTT connection, an wrapper around MQTTNet.IManagedMqttClient. 
    /// </summary>
    public class MqttConnection : IMqttConnection, IMqttApplicationMessageReceivedHandler, IApplicationMessageProcessedHandler, IMqttClientConnectedHandler, IMqttClientDisconnectedHandler, IConnectingFailedHandler, ISynchronizingSubscriptionsFailedHandler
    {
        private readonly IManagedMqttClientFactory _mqttClientFactory;
        private readonly MqttConfiguration _config;
        private readonly ILogger _logger;
        private readonly object startupLock = new object();
        private IManagedMqttClient _managedMqttClient;
        private IProcesMqttMessage _messageHandler;

        public MqttConnection(IManagedMqttClientFactory mqttClientFactory, MqttConfiguration config, ILogger logger)
        {
            _mqttClientFactory = mqttClientFactory;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current status of the connection.
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// Gets the descriptor for this Connection.
        /// </summary> ;
        public override string ToString()
        {
            return $"Connection for config: {_config.ToString()}, currently connected: {ConnectionState}";
        }

        /// <summary>
        /// Opens the MQTT connection.
        /// </summary>
        public async Task StartAsync(IProcesMqttMessage messageHandler)
        {
            try
            {
                lock (startupLock)
                {
                    if (_managedMqttClient != null || ConnectionState == ConnectionState.Connected)
                    {
                        return;
                    }
                    _messageHandler = messageHandler;

                    ConnectionState = ConnectionState.Connecting;
                    _managedMqttClient = _mqttClientFactory.CreateManagedMqttClient();
                    _managedMqttClient.ApplicationMessageReceivedHandler = this;
                    _managedMqttClient.ApplicationMessageProcessedHandler = this;
                    _managedMqttClient.ConnectedHandler = this;
                    _managedMqttClient.ConnectingFailedHandler = this;
                    _managedMqttClient.SynchronizingSubscriptionsFailedHandler = this;
                    _managedMqttClient.DisconnectedHandler = this;
                }
                await _managedMqttClient.StartAsync(_config.Options).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogCritical(new EventId(0), e, $"Exception while setting up the mqttclient to {this}");
                throw new MqttConnectionException($"Exception while setting up the mqttclient to {this}", e);
            }
        }

        public Task HandleApplicationMessageProcessedAsync(ApplicationMessageProcessedEventArgs eventArgs)
        {
            if (eventArgs?.HasFailed ?? throw new ArgumentNullException(nameof(eventArgs)))
            {
                _logger.LogError(new EventId(0), eventArgs.Exception, $"Message could not be processed for {this}, message: '{eventArgs.Exception?.Message}'");
            }
            return Task.CompletedTask;
        }

        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            ConnectionState = ConnectionState.Disconnected;
            _logger.LogWarning(new EventId(0), eventArgs?.Exception, $"MqttConnection Disconnected, previous connectivity state '{eventArgs?.ClientWasConnected}' for {this}, message: '{eventArgs?.Exception?.Message}'");
            return Task.CompletedTask;
        }

        public Task HandleConnectingFailedAsync(ManagedProcessFailedEventArgs eventArgs)
        {
            ConnectionState = ConnectionState.Disconnected;
            _logger.LogWarning(new EventId(0), eventArgs?.Exception, $"MqttConnection could not connect for {this}, message: '{eventArgs?.Exception?.Message}'");
            return Task.CompletedTask;
        }

        public Task HandleSynchronizingSubscriptionsFailedAsync(ManagedProcessFailedEventArgs eventArgs)
        {
            ConnectionState = ConnectionState.Disconnected;
            _logger.LogWarning(new EventId(0), eventArgs?.Exception, $"Subscription synchronization for {this} failed, message: '{eventArgs?.Exception?.Message}'");
            return Task.CompletedTask;
        }

        public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }
            
            if (eventArgs.ConnectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                ConnectionState = ConnectionState.Connected;
                _logger.LogInformation($"MqttConnection Connected for {this}");
            }
            else
            {
                ConnectionState = ConnectionState.Disconnected;
                _logger.LogWarning($"MqttConnection could not connect, result code: {eventArgs.ConnectResult.ResultCode} for {this}");
            }
            return Task.CompletedTask;
        }

        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }

            _logger.LogDebug($"MqttConnection receiving message for {this}");
            try
            {
                var qos = (MqttQualityOfServiceLevel)Enum.Parse(typeof(MqttQualityOfServiceLevel), eventArgs.ApplicationMessage.QualityOfServiceLevel.ToString());

                var mqttMessage = new MqttMessage(
                    eventArgs.ApplicationMessage.Topic,
                    eventArgs.ApplicationMessage.Payload,
                    qos,
                    eventArgs.ApplicationMessage.Retain);

                if (_messageHandler == null)
                {
                    var errormessage = $"MqttConnection receiving message but there is no handler available to process this message for {this}";
                    _logger.LogCritical(new EventId(0), errormessage);
                    throw new MqttConnectionException(errormessage);
                }

                await _messageHandler.OnMessage(new MqttMessageReceivedEventArgs(mqttMessage)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogCritical(new EventId(0), e, $"Exception while processing message for {this}");
                throw new MqttConnectionException($"Exception while processing message for {this}", e);
            }
        }

        /// <summary>
        /// Subscribe to one or more topics.
        /// </summary>
        /// <param name="topics">The topics to subscribe to.</param>
        public async Task SubscribeAsync(MqttTopicFilter[] topics)
        {
            if (_managedMqttClient == null)
            {
                throw new MqttConnectionException("Connection not open, please use StartAsync first!");
            }
            await _managedMqttClient.SubscribeAsync(topics).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe to one or more topics.
        /// </summary>
        /// <param name="topics">The topics to unsubscribe from.</param>
        public async Task UnubscribeAsync(string[] topics)
        {
            if (_managedMqttClient == null)
            {
                return;
            }
            await _managedMqttClient.UnsubscribeAsync(topics).ConfigureAwait(false);
        }

        /// <summary>
        /// Publish a message on to the MQTT broker.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        public async Task PublishAsync(MqttApplicationMessage message)
        {
            if (_managedMqttClient == null)
            {
                throw new MqttConnectionException("Connection not open, please use StartAsync first!");
            }
            await _managedMqttClient.PublishAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Close the MQTT connection.
        /// </summary>
        public Task StopAsync()
        {
            if (_managedMqttClient == null)
            {
                return Task.CompletedTask;
            }

            ConnectionState = ConnectionState.Disconnected;

            _managedMqttClient.StopAsync().Wait();
            _managedMqttClient.Dispose();
            _managedMqttClient = null;

            _messageHandler = null;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _managedMqttClient != null)
            {
                _managedMqttClient.Dispose();
                _managedMqttClient = null;
            }
        } 
    }
}
