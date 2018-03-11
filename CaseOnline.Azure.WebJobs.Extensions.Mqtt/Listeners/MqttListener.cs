using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks; 

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    [Singleton(Mode = SingletonMode.Listener)]
    public sealed class MqttListener : IListener
    {
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly MqttConfiguration _config;
        private bool _disposed;
        private IMqttClient _client;
        private IMqttClientOptions _options;

        public MqttListener(MqttConfiguration config, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            _config = config;
            _executor = executor;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Starting MqttListener");
            ThrowIfDisposed();

            await StartMqtt();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Stopping MqttListener");

            ThrowIfDisposed();

            if (_client == null)
            {
                throw new InvalidOperationException("The listener has not yet been started or has already been stopped.");
            }

            _cancellationTokenSource.Cancel();

            _client.DisconnectAsync().Wait();
            _client.Dispose();
            _client = null;

            return Task.FromResult<bool>(true);
        }

        public void Cancel()
        {
            _logger.LogTrace("MqttListener canceled");

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
                    _client.DisconnectAsync().Wait();
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

            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(_config.ServerUrl)
                .WithCredentials(_config.Username, _config.Password)
                //.WithTls()
                //.WithCleanSession()
                .Build();
            _client.ApplicationMessageReceived += _client_ApplicationMessageReceived; 
            _client.Disconnected += client_Disconnected;
            _client.Connected += _client_Connected; 
            await _client.ConnectAsync(_options);
             
            if (!_client.IsConnected) throw new Exception("Not able to connect to Mqtt server");
        }

        private async void client_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning("MqttDisconnected");

            await Task.Delay(TimeSpan.FromSeconds(1));

            try
            {
                await _client.ConnectAsync(_options);
            }
            catch
            {
                _logger.LogWarning("Reconnecting failed");
            }
        }

        private async void _client_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            await _client.SubscribeAsync(new TopicFilter[] { new TopicFilter(_config.Topic, MqttQualityOfServiceLevel.AtLeastOnce) });
        }

        private void _client_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogTrace("Mqtt client receiving message");
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
                _logger.LogError("Error firing function", e);
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
