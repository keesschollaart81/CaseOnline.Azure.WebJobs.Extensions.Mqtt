namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    public interface IMqttConnection : IDisposable
    {
        event Func<MqttMessageReceivedEventArgs, Task> OnMessageEventHandler;

        ConnectionState ConnectionState { get; }

        Task StartAsync();

        Task PublishAsync(MqttApplicationMessage message);

        Task StopAsync();

        Task SubscribeAsync(TopicFilter[] topics);

        Task UnubscribeAsync(string[] topics);
    }
}
