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
        private bool _disposed;
        private IManagedMqttClient _client;
        private IMqttClientOptions _options;

        public MqttListener(MqttConfiguration config, ITriggeredFunctionExecutor executor, TraceWriter logger)
        {
            _config = config;
            _executor = executor;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
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
                var options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId(_config.ClientId)
                        .WithTcpServer(_config.Server, _config.Port)
                        .WithCredentials(_config.Username, _config.Password)
                        .WithCleanSession()
                        //.WithTls()
                        .Build())
                    .Build();

                _client = new MqttFactory().CreateManagedMqttClient();
                _client.ApplicationMessageReceived += _client_ApplicationMessageReceived;
                _client.Connected += _client_Connected;
                _client.Disconnected += _client_Disconnected;
                await _client.SubscribeAsync(_config.Topics.Select(x => new TopicFilter(x, MqttQualityOfServiceLevel.AtLeastOnce)));
                await _client.StartAsync(options); 
            }
            catch (Exception e)
            {
                _logger.Error("Unhandled exception while connectin to Mqtt server", e);
                throw new Exception("Unhandled exception while connectin to Mqtt server", e);
            }
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
            var input = new TriggeredFunctionData
            {
                TriggerValue = MqttInfo
            };

            try
            {
                FunctionResult result = await _executor.TryExecuteAsync(input, token);
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
