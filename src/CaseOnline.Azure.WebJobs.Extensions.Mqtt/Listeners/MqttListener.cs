using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    [Singleton(Mode = SingletonMode.Listener)]
    public sealed class MqttListener : IListener
    {
        private readonly ITriggeredFunctionExecutor _executor; 
        private readonly TraceWriter _traceWriter;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly MqttConfiguration _config;
        private readonly IMqttClientFactory _mqttClientFactory;
        private readonly Timer _timer;
        private bool _disposed;
        private IManagedMqttClient _managedMqttClient;

        public MqttListener(IMqttClientFactory mqttClientFactory, MqttConfiguration config, ITriggeredFunctionExecutor executor, TraceWriter traceWriter)
        {
            _config = config;
            _mqttClientFactory = mqttClientFactory;
            _executor = executor; 
            _traceWriter = traceWriter;
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new Timer(Execute, null, 30000, Timeout.Infinite); 
        }

        private string Descriptor => $"client {_config?.Options?.ClientOptions?.ClientId} and topics {string.Join(",", _config?.Topics?.Select(t => t.Topic))}";

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _traceWriter.Info($"Starting MqttListener for {Descriptor}");
            ThrowIfDisposed();

            await StartMqtt(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _traceWriter.Info($"Stopping MqttListener for {Descriptor}");

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
            _traceWriter.Info($"Cancelling MqttListener for {Descriptor}");

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
                _traceWriter.Error("Unhandled exception while connectin to {descriptor}", e);
                throw new Exception("Unhandled exception while connectin to {descriptor}", e);
            }
        }

        public void Execute(object stateInfo)
        {
            _traceWriter.Verbose($"Timer: {_managedMqttClient?.IsConnected} {DateTime.Now:g} {Descriptor}");
            _timer.Change(30000, Timeout.Infinite);
        }

        private void ManagedMqttClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _traceWriter.Error($"MqttListener._client_Disconnected, was :{e.ClientWasConnected} for {Descriptor}", e.Exception);
        }

        private void ManagedMqttClientConnected(object sender, MqttClientConnectedEventArgs e)
        {
            _traceWriter.Info($"MqttListener._client_Connected {e.IsSessionPresent} for {Descriptor}");
        }

        private void ManagedMqttClientApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            _traceWriter.Info("Mqtt client receiving message for {descriptor}");
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
                FunctionResult result = await _executor.TryExecuteAsync(triggeredFunctionData, token).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (Exception e)
            {
                _traceWriter.Error("Error firing function", e);

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
