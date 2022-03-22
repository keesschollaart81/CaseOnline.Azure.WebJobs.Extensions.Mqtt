namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;

public enum MqttRetainHandling
{
    SendAtSubscribe = 0,
    SendAtSubscribeIfNewSubscriptionOnly = 1,
    DoNotSendOnSubscribe = 2,
    NotSet
}
