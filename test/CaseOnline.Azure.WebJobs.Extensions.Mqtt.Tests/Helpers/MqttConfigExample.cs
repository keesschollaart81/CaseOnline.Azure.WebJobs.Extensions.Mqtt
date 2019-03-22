using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
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
