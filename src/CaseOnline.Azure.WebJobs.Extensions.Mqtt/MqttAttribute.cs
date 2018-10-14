using System;
using Microsoft.Azure.WebJobs.Description;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class MqttAttribute : MqttBaseAttribute
    {
        public MqttAttribute()
        {
        }

        public MqttAttribute(Type mqttConfigCreatorType) : base(mqttConfigCreatorType)
        {
        }
    }
}
