using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    /// <summary>
    /// Listens for MQTT messages.
    /// </summary>
    [Singleton(Mode = SingletonMode.Listener)]
    public sealed class MqttListener : IListener
    { 
        private readonly ITriggeredFunctionExecutor _executor; 
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly MqttConfiguration _config;
        private readonly IMqttClientFactory _mqttClientFactory;
        private bool _disposed;
        private IManagedMqttClient _managedMqttClient;

        /// <summary>
        /// Inititalizes a new instance of the <see cref="MqttListener"/> class.
        /// </summary>
        /// <param name="mqttClientFactory">The factory for <see cref="IManagedMqttClient"/>s.</param>
        /// <param name="config">The MQTT configuration.</param>
        /// <param name="executor">Allows the function to be executed.</param>
        /// <param name="logger">The logger.</param>
        public MqttListener(IMqttClientFactory mqttClientFactory, MqttConfiguration config, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            _config = config;
            _mqttClientFactory = mqttClientFactory;
            _executor = executor; 
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Gets a value indicating whether the listener is connected to the MQTT queue.
        /// </summary>
        public static bool Connected { get; private set; }

        /// <summary>
        /// Gets the descriptor for this listener.
        /// </summary>
        private string Descriptor => $"Client {_config?.Options?.ClientOptions?.ClientId} and topics {string.Join(", ", _config?.Topics?.Select(t => t.Topic))}";

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting MqttListener for {Descriptor}");
            ThrowIfDisposed();

            await StartMqtt(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping MqttListener for {Descriptor}");

            ThrowIfDisposed();

            if (_managedMqttClient == null)
            {
                throw new InvalidOperationException("The listener has not yet been started or has already been stopped.");
            }

            _cancellationTokenSource.Cancel();

            _managedMqttClient.StopAsync().Wait();
            _managedMqttClient.Dispose();
            _managedMqttClient = null;

            return Task.FromResult<bool>(true);
        }

        public void Cancel()
        {
            _logger.LogWarning($"Cancelling MqttListener for {Descriptor}");

            ThrowIfDisposed();
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cancellationTokenSource.Cancel();

                if (_managedMqttClient != null)
                {
                    _managedMqttClient.StopAsync().Wait();
                    _managedMqttClient.Dispose();
                    _managedMqttClient = null;
                }

                _disposed = true;
            }
        }

        private async Task StartMqtt(CancellationToken cancellationToken)
        {
            if (_cancellationTokenSource.IsCancellationRequested || cancellationToken.IsCancellationRequested)
            {
                return;
            }
            try
            {
                _managedMqttClient = _mqttClientFactory.CreateManagedMqttClient();
                _managedMqttClient.ApplicationMessageReceived += ManagedMqttClientApplicationMessageReceived;
                _managedMqttClient.Connected += ManagedMqttClientConnected;
                _managedMqttClient.Disconnected += ManagedMqttClientDisconnected;

                await _managedMqttClient.SubscribeAsync(_config.Topics).ConfigureAwait(false);
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
            _logger.LogWarning($"MqttListener Disconnected, previous connectivity state '{e.ClientWasConnected}' for {Descriptor}, message: {e.Exception?.Message}", e.Exception);
        }

        private void ManagedMqttClientConnected(object sender, MqttClientConnectedEventArgs e)
        {
            Connected = true;
            _logger.LogInformation($"MqttListener Connected, IsSessionPresent: '{e.IsSessionPresent}' for {Descriptor}");
        }

        private void ManagedMqttClientApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogDebug($"MqttListener receiving message for {Descriptor}");
            InvokeJobFunction(e).Wait();
        }

        private async Task InvokeJobFunction(MqttApplicationMessageReceivedEventArgs mqttApplicationMessageReceivedEventArgs)
        {
            var token = _cancellationTokenSource.Token;

            var mqttInfo = new PublishedMqttMessage(
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Topic,
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Payload,
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.QualityOfServiceLevel.ToString(),
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Retain);

            var triggeredFunctionData = new TriggeredFunctionData
            {
                TriggerValue = mqttInfo
            };

            try
            {
                var result = await _executor.TryExecuteAsync(triggeredFunctionData, token).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    if (!token.IsCancellationRequested)
                    {
                        _logger.LogCritical("Error firing function", result.Exception);
                    }
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical("Error firing function", e);

                // We don't want any function errors to stop the execution. Errors will be logged to Dashboard already.
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}
