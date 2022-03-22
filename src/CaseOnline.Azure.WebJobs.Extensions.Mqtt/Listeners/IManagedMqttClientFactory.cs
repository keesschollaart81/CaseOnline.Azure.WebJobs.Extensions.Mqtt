using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;

public interface IManagedMqttClientFactory
{
    IManagedMqttClient CreateManagedMqttClient();
}
