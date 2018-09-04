using System;
using Microsoft.Azure.WebJobs.Description;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class MqttAttribute : Attribute, IRquireMqttConnection
    {
        public MqttAttribute()
        {
        }

        public MqttAttribute(Type mqttConfigCreatorType)
        {
            MqttConfigCreatorType = mqttConfigCreatorType;
        }

        public string ConnectionString { get; set; }

        public Type MqttConfigCreatorType { get; }
    }
}
