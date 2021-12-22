using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    /// <summary>
    /// Listens for MQTT messages.
    /// </summary>
    [Singleton(Mode = SingletonMode.Listener)]
    public sealed class MqttListener : IListener, IProcesMqttMessage
    {
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IMqttConnection _mqttConnection;
        private readonly MqttTopicFilter[] _topics;
        private bool _disposed;

        /// <summary>
        /// Inititalizes a new instance of the <see cref="MqttListener"/> class.
        /// </summary>
        /// <param name="connection">The connection to listen on.</param>
        /// <param name="topics">The topics to subscribe to.</param> 
        /// <param name="executor">Allows the function to be executed.</param>
        /// <param name="logger">The logger.</param>
        public MqttListener(IMqttConnection connection, MqttTopicFilter[] topics, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            _executor = executor;
            _logger = logger;
            _mqttConnection = connection;
            _topics = topics;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting MqttListener for {_mqttConnection}");

            ThrowIfDisposed();

            if (_cancellationTokenSource.IsCancellationRequested || cancellationToken.IsCancellationRequested)
            {
                return;
            }
            await _mqttConnection.StartAsync(this).ConfigureAwait(false);

            await _mqttConnection.SubscribeAsync(_topics).ConfigureAwait(false);
        }

        public async Task OnMessage(MqttMessageReceivedEventArgs arg)
        {
            var token = _cancellationTokenSource.Token;
           
            var triggeredFunctionData = new TriggeredFunctionData
            {
                TriggerValue = arg?.Message ?? throw new ArgumentNullException(nameof(arg))
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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping MqttListener for {_mqttConnection}");

            ThrowIfDisposed();

            _cancellationTokenSource.Cancel();

            await _mqttConnection.UnubscribeAsync(_topics.Select(x => x.Topic).ToArray()).ConfigureAwait(false);

            await _mqttConnection.StopAsync().ConfigureAwait(false);
        }

        public void Cancel()
        {
            _logger.LogWarning($"Cancelling MqttListener for {_mqttConnection}");

            ThrowIfDisposed();

            StopAsync(_cancellationTokenSource.Token).Wait();

            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _mqttConnection.Dispose();
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _disposed = true;
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
