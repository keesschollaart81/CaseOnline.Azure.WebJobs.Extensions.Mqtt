using System;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttMessageCollector : IAsyncCollector<IMqttMessage>
    {
        private readonly MqttConnection _mqttConnection;

        public MqttMessageCollector(MqttConnection mqttConnection)
        {
            _mqttConnection = mqttConnection;
        }

        public async Task AddAsync(IMqttMessage item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_mqttConnection.ConnectionState != ConnectionState.Connected)
            {
                IProcesMqttMessage messageProcessor = null!; // this is only for publising, we dont expect incoming messages
                await _mqttConnection.StartAsync(messageProcessor).ConfigureAwait(false);
                for (var i = 0; i < 100; i++)
                {
                    if (_mqttConnection.ConnectionState != ConnectionState.Connected)
                    {
                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    } 
                }
            }
            var qos = (MQTTnet.Protocol.MqttQualityOfServiceLevel)Enum.Parse(typeof(MQTTnet.Protocol.MqttQualityOfServiceLevel), item.QosLevel.ToString());
            var mqttApplicationMessage = new MqttApplicationMessage
            {
                Topic = item.Topic,
                Payload = item.GetMessage(),
                QualityOfServiceLevel = qos,
                Retain = item.Retain
            };
            await _mqttConnection.PublishAsync(mqttApplicationMessage).ConfigureAwait(false);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}
