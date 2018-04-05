using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    public interface IMqttConnection
    {
        bool Connected { get; }

        event Func<MqttMessageReceivedEventArgs, Task> OnMessageEventHandler;

        Task StartAsync();

        Task PublishAsync(MqttApplicationMessage message);

        Task StopAsync(CancellationToken cancellationToken);

        Task SubscribeAsync(TopicFilter[] topics);

        Task UnubscribeAsync(string[] topics);
    }
}
