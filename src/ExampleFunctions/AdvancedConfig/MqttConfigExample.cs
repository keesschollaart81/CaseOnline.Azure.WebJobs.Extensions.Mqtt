using MQTTnet.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet;

namespace ExampleFunction.AdvancedConfig
{
    public class MqttConfigExample : MqttConfig
    {
        public override IManagedMqttClientOptions Options { get; }

        public override TopicFilter[] Topics { get; }

        public override string ServerUrl { get; }

        public MqttConfigExample(string serverUrl, IManagedMqttClientOptions options, TopicFilter[] topics)
        {
            ServerUrl = serverUrl;
            Options = options;
            Topics = topics;
        }
    }
}
