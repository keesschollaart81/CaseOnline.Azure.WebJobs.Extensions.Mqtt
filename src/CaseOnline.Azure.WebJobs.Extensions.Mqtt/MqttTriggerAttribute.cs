using System;
using Microsoft.Azure.WebJobs.Description;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class MqttTriggerAttribute : MqttBaseAttribute
    {
        public MqttTriggerAttribute(params string[] topics)
        {
            Topics = topics;
        }

        public MqttTriggerAttribute(Type mqttConfigCreatorType, params string[] topics) : base(mqttConfigCreatorType)
        {
            Topics = topics;
        }

        public string[] Topics { get; }
    }
}
