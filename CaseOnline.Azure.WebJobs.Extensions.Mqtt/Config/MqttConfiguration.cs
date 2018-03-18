using System;
using MQTTnet;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConfiguration : Attribute
    {
        public IManagedMqttClientOptions Options { get; }

        public TopicFilter[] Topics { get; }

        public MqttConfiguration(IManagedMqttClientOptions options, TopicFilter[] topics)
        {
            Options = options;
            Topics = topics;
        }
    }
}
