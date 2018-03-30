namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    /// <summary>
    /// A published MQTT message.
    /// </summary>
    public class PublishedMqttMessage
    {
        private readonly byte[] _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedMqttMessage"/> class.
        /// </summary>
        public PublishedMqttMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedMqttMessage"/> class.
        /// </summary>
        internal PublishedMqttMessage(string topic, byte[] message, string qosLevel, bool retain)
        {
            Topic = topic;
            _message = message;
            QosLevel = qosLevel;
            Retain = retain;
        }

        /// <summary>
        /// Gets the topic of the message.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Gets the Quality of Service level.
        /// </summary>
        public string QosLevel { get; }

        /// <summary>
        /// Gets a value indicating whether to retain this message.
        /// </summary>
        public bool Retain { get; }

        /// <summary>
        /// Gets the messages as an array of <see cref="byte"/>.
        /// </summary>
        /// <returns>The message as an array of <see cref="byte"/>.</returns>
        public byte[] GetMessage()
        {
            return _message;
        }
    }
}
