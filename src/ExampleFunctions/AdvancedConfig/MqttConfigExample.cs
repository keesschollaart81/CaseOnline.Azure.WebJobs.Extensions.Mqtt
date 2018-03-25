using MQTTnet.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet;
using System;
using System.Collections.Generic;

namespace ExampleFunction.AdvancedConfig
{
    public class MqttConfigExample : MqttConfig
    {
        public override IManagedMqttClientOptions Options { get; }

        public override IEnumerable<TopicFilter> Topics { get; }

        public MqttConfigExample(IManagedMqttClientOptions options, IEnumerable<TopicFilter> topics)
        {
            Options = options;
            Topics = topics;
        }
    }
}
