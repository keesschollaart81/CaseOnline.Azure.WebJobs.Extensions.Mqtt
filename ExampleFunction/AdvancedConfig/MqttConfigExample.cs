using MQTTnet.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet;

namespace ExampleFunction.AdvancedConfig
{
    public class MqttConfigExample : MqttConfig
    {
        public override IManagedMqttClientOptions Options { get;   }
        
        public override TopicFilter[] Topics { get; }

        public MqttConfigExample(IManagedMqttClientOptions options, TopicFilter[] topics )
        {
            Options = options;
            Topics = topics;
        }
    }
}
