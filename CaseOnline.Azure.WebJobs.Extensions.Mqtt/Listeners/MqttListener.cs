using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

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
        private MqttClient _client;

        public MqttListener(MqttConfiguration config, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            _config = config;
            _executor = executor;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            StartMqtt();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (_client == null)
            {
                throw new InvalidOperationException("The listener has not yet been started or has already been stopped.");
            }

            _cancellationTokenSource.Cancel();

            _client.Disconnect();
            _client = null;

            return Task.FromResult<bool>(true);
        }

        public void Cancel()
        {
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
                    _client.Disconnect();
                    _client = null;
                }

                _disposed = true;
            }
        }

        private void StartMqtt()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            _client = new MqttClient(_config.ServerUrl);

            _client.MqttMsgPublishReceived += mqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            _client.Connect(clientId, _config.Username, _config.Password);

            _client.Subscribe(new string[] { _config.Topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        }

        private void mqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs mqttMsgPublishEventArgs)
        {
            InvokeJobFunction(mqttMsgPublishEventArgs).Wait();
        }

        private async Task InvokeJobFunction(MqttMsgPublishEventArgs mqttMsgPublishEventArgs)
        {
            var token = _cancellationTokenSource.Token;

            var MqttInfo = new PublishedMqttMessage(mqttMsgPublishEventArgs.Topic, mqttMsgPublishEventArgs.Message, mqttMsgPublishEventArgs.DupFlag, mqttMsgPublishEventArgs.QosLevel, mqttMsgPublishEventArgs.Retain);
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
            catch
            {
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
