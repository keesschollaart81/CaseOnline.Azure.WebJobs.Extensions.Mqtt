using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using MQTTnet;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;
using System;
using System.Linq;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{

    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class MqttTriggerAttribute : Attribute
    {
        public readonly IManagedMqttClientOptions ManagedMqttClientOptions;

        public readonly TopicFilter[] Topics;

        public bool UseManagedMqttClient => ManagedMqttClientOptions != null;

        [AppSetting]
        public string ServerName { get; set; }

        [AppSetting]
        public string PortName { get; set; }

        [AppSetting]
        public string UsernameName { get; set; }

        [AppSetting]
        public string PasswordName { get; set; }

        [AppSetting]
        public string ClientIdName { get; set; }

        public TimeSpan ReconnectDelay { get; }

        public IManagedMqttClientOptions ManagedMqttSettings => ManagedMqttClientOptions;

        public MqttTriggerAttribute(string[] topics) : this(topics, TimeSpan.FromSeconds(5))
        {
        }

        public MqttTriggerAttribute(string[] topics, TimeSpan reconnectDelay)
        {
            Topics = topics.Select(x => new TopicFilter(x, MqttQualityOfServiceLevel.AtLeastOnce)).ToArray();
            ReconnectDelay = reconnectDelay;
        }

        public MqttTriggerAttribute(IManagedMqttClientOptions managedMqttClientOptions, TopicFilter[] topics)
        {
            ManagedMqttClientOptions = managedMqttClientOptions;
            Topics = topics;
        }
        public MqttTriggerAttribute(Type type)
        { 
        }
    }
}
