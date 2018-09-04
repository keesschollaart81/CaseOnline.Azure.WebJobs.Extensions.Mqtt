using System;
using Microsoft.Azure.WebJobs.Description;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class MqttTriggerAttribute : Attribute, IRquireMqttConnection
    {
        public MqttTriggerAttribute(params string[] topics)
        {
            Topics = topics;
        }

        public MqttTriggerAttribute(Type mqttConfigCreatorType, params string[] topics)
        {
            MqttConfigCreatorType = mqttConfigCreatorType;
            Topics = topics;
        }

        public Type MqttConfigCreatorType { get; }

        public string[] Topics { get; }
        
        [AppSetting]
        public string ConnectionString { get; set; }
    }
}
