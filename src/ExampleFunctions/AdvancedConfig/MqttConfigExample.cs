using MQTTnet.Extensions.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;

namespace ExampleFunction.AdvancedConfig
{
    public class MqttConfigExample : CustomMqttConfig
    {
        public override IManagedMqttClientOptions Options { get; }

        public override string Name { get; }
        
        public MqttConfigExample(string name, IManagedMqttClientOptions options)
        {
            Options = options;
            Name = name;
        }
    }
}
