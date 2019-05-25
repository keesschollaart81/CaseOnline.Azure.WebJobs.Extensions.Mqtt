using System.Threading.Tasks;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    public interface IProcesMqttMessage
    {
        Task OnMessage(MqttMessageReceivedEventArgs arg);
    }
}
