using Microsoft.Azure.WebJobs.Description;
using MQTTnet.Protocol;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Binding]
public class MqttTriggerAttribute : MqttBaseAttribute
{
    internal MqttTriggerAttribute()
    {
    }

    public MqttTriggerAttribute(params string[] topics)
    {
        TopicStrings = topics;
    }

    public MqttTriggerAttribute(string topic, Messaging.MqttQualityOfServiceLevel qos = Messaging.MqttQualityOfServiceLevel.AtLeastOnce, Messaging.NoLocal noLocal = Messaging.NoLocal.NotSet, Messaging.RetainAsPublished retainAsPublished = Messaging.RetainAsPublished.NotSet, Messaging.MqttRetainHandling retainHandling = Messaging.MqttRetainHandling.NotSet)
    {
        TopicFilter = new MqttTopicFilter
        {
            Topic = topic,
            NoLocal = noLocal switch
            {
                Messaging.NoLocal.True => true,
                Messaging.NoLocal.False => false,
                _ => false
            },
            QualityOfServiceLevel = qos switch
            {
                Messaging.MqttQualityOfServiceLevel.AtLeastOnce => MqttQualityOfServiceLevel.AtLeastOnce,
                Messaging.MqttQualityOfServiceLevel.AtMostOnce => MqttQualityOfServiceLevel.AtMostOnce,
                Messaging.MqttQualityOfServiceLevel.ExactlyOnce => MqttQualityOfServiceLevel.ExactlyOnce,
                _ => MqttQualityOfServiceLevel.AtLeastOnce
            },
            RetainAsPublished = retainAsPublished switch
            {
                Messaging.RetainAsPublished.True => true,
                Messaging.RetainAsPublished.False => false,
                _ => false
            },
            RetainHandling = retainHandling switch
            {
                Messaging.MqttRetainHandling.DoNotSendOnSubscribe => MqttRetainHandling.DoNotSendOnSubscribe,
                Messaging.MqttRetainHandling.SendAtSubscribe => MqttRetainHandling.SendAtSubscribe,
                Messaging.MqttRetainHandling.SendAtSubscribeIfNewSubscriptionOnly => MqttRetainHandling.SendAtSubscribeIfNewSubscriptionOnly,
                _ => MqttRetainHandling.DoNotSendOnSubscribe,
            }
        };
    }

    public MqttTriggerAttribute(Type mqttConfigCreatorType, params string[] topics) : base(mqttConfigCreatorType)
    {
        TopicStrings = topics;
    }

    public MqttTopicFilter TopicFilter { get; }

    internal string[] TopicStrings { get; }
}
