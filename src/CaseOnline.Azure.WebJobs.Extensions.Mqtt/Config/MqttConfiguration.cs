using System;
using MQTTnet;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConfiguration
    {
        public string ServerUrl { get; }

        public IManagedMqttClientOptions Options { get; }

        public TopicFilter[] Topics { get; }

        public MqttConfiguration(string serverUrl, IManagedMqttClientOptions options, TopicFilter[] topics)
        { 
            ServerUrl = serverUrl ?? throw new ArgumentNullException(nameof(serverUrl));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Topics = topics ?? throw new ArgumentNullException(nameof(topics));
        }
    }
}
