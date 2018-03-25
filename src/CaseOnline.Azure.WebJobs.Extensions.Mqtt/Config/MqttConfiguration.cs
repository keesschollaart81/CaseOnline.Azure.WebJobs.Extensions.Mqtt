using System;
using System.Collections.Generic;
using MQTTnet;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConfiguration
    {
        public MqttConfiguration(Uri serverUrl, IManagedMqttClientOptions options, IEnumerable<TopicFilter> topics)
        {
            ServerUrl = serverUrl ?? throw new ArgumentNullException(nameof(serverUrl));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Topics = topics ?? throw new ArgumentNullException(nameof(topics));
        }

        public Uri ServerUrl { get; }

        public IManagedMqttClientOptions Options { get; }

        public IEnumerable<TopicFilter> Topics { get; }
    }
}
