using System;
using System.Collections.Generic;
using MQTTnet;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    /// <summary>
    /// The configuration for MQTT.
    /// </summary>
    public class MqttConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MqttConfiguration"/> class.
        /// </summary>
        /// <param name="options">The managed options.</param>
        /// <param name="topics">The topic filters.</param>
        public MqttConfiguration(IManagedMqttClientOptions options, IEnumerable<TopicFilter> topics)
        { 
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Topics = topics ?? throw new ArgumentNullException(nameof(topics));
        }

        /// <summary>
        /// Gets the managed options.
        /// </summary>
        public IManagedMqttClientOptions Options { get; }

        /// <summary>
        /// Gets the topic filters.
        /// </summary>
        public IEnumerable<TopicFilter> Topics { get; }
    }
}
