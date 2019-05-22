using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    public class ManagedMqttClientFactory : IManagedMqttClientFactory
    {
        private readonly IMqttFactory _mqttFactory;

        public ManagedMqttClientFactory(IMqttFactory mqttFactory)
        {
            _mqttFactory = mqttFactory;
        }

        public IManagedMqttClient CreateManagedMqttClient()
        {
            return _mqttFactory.CreateManagedMqttClient();
        }
    }
}
