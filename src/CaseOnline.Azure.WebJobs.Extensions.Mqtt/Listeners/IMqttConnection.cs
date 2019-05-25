using System;
using System.Threading.Tasks;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    public interface IMqttConnection : IDisposable
    {
        ConnectionState ConnectionState { get; }

        Task StartAsync(IProcesMqttMessage messageHandler);

        Task PublishAsync(MqttApplicationMessage message);

        Task StopAsync();

        Task SubscribeAsync(TopicFilter[] topics);

        Task UnubscribeAsync(string[] topics);
    }
}
