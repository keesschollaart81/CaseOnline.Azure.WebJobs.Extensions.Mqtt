namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    public class PublishedMqttMessage
    {
        public string Topic { get; }
        public byte[] Message { get; } 
        public string QosLevel { get; }
        public bool Retain { get; }

        public PublishedMqttMessage()
        {

        }

        public PublishedMqttMessage(string topic, byte[] message, string qosLevel, bool retain)
        {
            Topic = topic;
            Message = message; 
            QosLevel = qosLevel;
            Retain = retain;
        }
    }
}
