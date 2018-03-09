namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    public class PublishedMqttMessage
    {
        public string Topic { get; }
        public byte[] Message { get; }
        public bool DupFlag { get; set; }
        public byte QosLevel { get; }
        public bool Retain { get; }

        public PublishedMqttMessage(string topic, byte[] message, bool dupFlag, byte qosLevel, bool retain)
        {
            Topic = topic;
            Message = message;
            DupFlag = dupFlag;
            QosLevel = qosLevel;
            Retain = retain;
        }
    }
}
