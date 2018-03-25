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

        public override Uri ServerUrl { get; }

        public MqttConfigExample(Uri serverUrl, IManagedMqttClientOptions options, IEnumerable<TopicFilter> topics)
        {
            ServerUrl = serverUrl;
            Options = options;
            Topics = topics;
        }
    }
}
