namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    public class PublishedMqttMessage
    {
        private readonly byte[] _message;

        public PublishedMqttMessage()
        {
        }

        public PublishedMqttMessage(string topic, byte[] message, string qosLevel, bool retain)
        {
            Topic = topic;
            _message = message;
            QosLevel = qosLevel;
            Retain = retain;
        }

        public string Topic { get; }

        public string QosLevel { get; }

        public bool Retain { get; }

        public byte[] GetMessage()
        {
            return _message;
        }
    }
}
