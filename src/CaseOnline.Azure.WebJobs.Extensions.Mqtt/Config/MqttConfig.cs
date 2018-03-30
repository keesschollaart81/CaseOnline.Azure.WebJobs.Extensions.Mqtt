using System.Collections.Generic;
using MQTTnet;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    /// <summary>
    /// Configuration for the MQTT binding.
    /// </summary>
    public abstract class MqttConfig
    {
        /// <summary>
        /// Gets the options.
        /// </summary>
        public abstract IManagedMqttClientOptions Options { get; }

        /// <summary>
        /// Gets the topics.
        /// </summary>
        public abstract IEnumerable<TopicFilter> Topics { get; }
    }
}
