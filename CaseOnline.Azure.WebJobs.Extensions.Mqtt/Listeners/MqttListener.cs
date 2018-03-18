using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    [Singleton(Mode = SingletonMode.Listener)]
    public sealed class MqttListener : IListener
    {
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly TraceWriter _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly MqttConfiguration _config;
        private readonly IMqttClientFactory _mqttClientFactory;
        private bool _disposed;
        private IManagedMqttClient _client;
        private readonly Timer _timer;

        public MqttListener(IMqttClientFactory mqttClientFactory, MqttConfiguration config, ITriggeredFunctionExecutor executor, TraceWriter logger)
        {
            _config = config;
            _mqttClientFactory = mqttClientFactory;
            _executor = executor;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new Timer(Execute, null, 30000, Timeout.Infinite);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Info("Starting MqttListener");
            ThrowIfDisposed();

            await StartMqtt();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Warning("Stopping MqttListener");

            ThrowIfDisposed();

            if (_client == null)
            {
                throw new InvalidOperationException("The listener has not yet been started or has already been stopped.");
            }

            _cancellationTokenSource.Cancel();

            _client.StopAsync().Wait();
            _client.Dispose();
            _client = null;

            return Task.FromResult<bool>(true);
        }

        public void Cancel()
        {
            _logger.Info("MqttListener canceled");

            ThrowIfDisposed();
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cancellationTokenSource.Cancel();

                if (_client != null)
                {
                    _client.StopAsync().Wait();
                    _client.Dispose();
                    _client = null;
                }

                _disposed = true;
            }
        }

        private async Task StartMqtt()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            try
            { 
                _client = _mqttClientFactory.CreateManagedMqttClient();
                _client.ApplicationMessageReceived += _client_ApplicationMessageReceived;
                _client.Connected += _client_Connected;
                _client.Disconnected += _client_Disconnected;
                await _client.SubscribeAsync(_config.Topics);
                await _client.StartAsync(_config.Options);
            }
            catch (Exception e)
            {
                _logger.Error("Unhandled exception while connectin to Mqtt server", e);
                throw new Exception("Unhandled exception while connectin to Mqtt server", e);
            } 
        }

        public void Execute(System.Object stateInfo)
        {
            _logger.Info($"Timer: {_client?.IsConnected} {DateTime.Now:g}");
            _timer.Change(30000, Timeout.Infinite);
        }
        private void _client_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _logger.Error($"MqttListener._client_Disconnected, was :{e.ClientWasConnected} ", e.Exception);
        }

        private void _client_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            _logger.Info($"MqttListener._client_Connected {e.IsSessionPresent}");
        }

        private void _client_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.Info("Mqtt client receiving message");
            InvokeJobFunction(e).Wait();
        }

        private async Task InvokeJobFunction(MqttApplicationMessageReceivedEventArgs mqttApplicationMessageReceivedEventArgs)
        {
            var token = _cancellationTokenSource.Token;

            var MqttInfo = new PublishedMqttMessage(
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Topic,
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Payload,
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.QualityOfServiceLevel.ToString(),
                mqttApplicationMessageReceivedEventArgs.ApplicationMessage.Retain);

            var triggeredFunctionData = new TriggeredFunctionData
            {
                TriggerValue = MqttInfo
            };

            try
            {
                FunctionResult result = await _executor.TryExecuteAsync(triggeredFunctionData, token);
                if (!result.Succeeded)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error firing function", e);
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
