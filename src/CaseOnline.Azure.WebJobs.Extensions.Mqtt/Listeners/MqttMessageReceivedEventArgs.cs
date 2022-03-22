using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;

public sealed class MqttMessageReceivedEventArgs : EventArgs
{
    public MqttMessageReceivedEventArgs(IMqttMessage message)
    {
        Message = message;
    }

    public IMqttMessage Message { get; }
}
