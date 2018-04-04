namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging
{
    /// <summary>
    /// A MQTT message.
    /// </summary>
    public interface IMqttMessage
    {
        /// <summary>
        /// Gets the topic of the message.
        /// </summary>
        string Topic { get; }

        /// <summary>
        /// Gets the Quality of Service level.
        /// </summary>
        MqttQualityOfServiceLevel QosLevel { get; }

        /// <summary>
        /// Gets a value indicating whether to retain this message.
        /// </summary>
        bool Retain { get; }

        /// <summary>
        /// Gets the messages as an array of <see cref="byte"/>.
        /// </summary>
        /// <returns>The message as an array of <see cref="byte"/>.</returns>
        byte[] GetMessage();
    }
}
