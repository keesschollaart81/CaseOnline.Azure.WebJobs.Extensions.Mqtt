using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    public interface IMqttConfigurationParser
    {
        MqttConfiguration Parse(MqttBaseAttribute mqttAttribute);
    }
}
