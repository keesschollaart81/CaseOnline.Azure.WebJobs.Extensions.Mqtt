using MQTTnet;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public abstract class MqttConfig
    {
        public abstract IManagedMqttClientOptions Options { get; }
        public abstract TopicFilter[] Topics { get; }
    }

}
