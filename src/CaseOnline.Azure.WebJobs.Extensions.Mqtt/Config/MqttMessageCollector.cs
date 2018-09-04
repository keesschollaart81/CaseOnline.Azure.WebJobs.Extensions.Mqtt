using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttMessageCollector : IAsyncCollector<IMqttMessage>
    {
        private readonly MqttAttribute _attr;
        private readonly MqttConnection _mqttConnection;

        public MqttMessageCollector(MqttAttribute attr, MqttConnection mqttConnection)
        {
            _attr = attr;
            _mqttConnection = mqttConnection;
        }

        public async Task AddAsync(IMqttMessage item, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_mqttConnection.ConnectionState != ConnectionState.Connected)
            {
                await _mqttConnection.StartAsync().ConfigureAwait(false);
                for (var i = 0; i < 100; i++)
                {
                    if (_mqttConnection.ConnectionState != ConnectionState.Connected)
                    {
                        await Task.Delay(50).ConfigureAwait(false);
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
