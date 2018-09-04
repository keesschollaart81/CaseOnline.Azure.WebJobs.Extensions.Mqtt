namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    public class ManagedMqttClientFactory : IManagedMqttClientFactory
    {
        private readonly IMqttClientFactory _mqttClientFactory;

        public ManagedMqttClientFactory(IMqttClientFactory mqttClientFactory)
        {
            _mqttClientFactory = mqttClientFactory;
        }

        public IManagedMqttClient CreateManagedMqttClient()
        {
            return _mqttClientFactory.CreateManagedMqttClient();
        }
    }
}
