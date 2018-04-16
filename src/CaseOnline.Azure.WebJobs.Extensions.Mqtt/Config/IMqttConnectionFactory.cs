using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public interface IMqttConnectionFactory
    {
        Task DisconnectAll();

        MqttConnection GetMqttConnection(IRquireMqttConnection attribute);
    }
}
