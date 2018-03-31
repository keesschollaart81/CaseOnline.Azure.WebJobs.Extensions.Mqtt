using System;
using Microsoft.Azure.WebJobs.Description;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class MqttTriggerAttribute : Attribute
    {
        public MqttTriggerAttribute()
        {
        }

        public MqttTriggerAttribute(params string[] topics)
        {
            Topics = topics;
        }

        public MqttTriggerAttribute(Type mqttConfigCreatorType)
        {
            MqttConfigCreatorType = mqttConfigCreatorType;
        }

        public Type MqttConfigCreatorType { get; }

        public string[] Topics { get; }

        public bool UseCustomConfigCreator => MqttConfigCreatorType != null;

        [AppSetting]
        public string ConnectionString { get; set; }
    }
}
