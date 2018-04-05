using System;
using System.Collections.Generic;
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

        public MqttAttribute(params string[] topics)
        {
            Topics = topics;
        }

        public MqttAttribute(Type mqttConfigCreatorType)
        {
            MqttConfigCreatorType = mqttConfigCreatorType;
        }

        public IEnumerable<string> Topics { get; }

        public string ConnectionString { get; set; }

        public Type MqttConfigCreatorType { get; }
    }
}
