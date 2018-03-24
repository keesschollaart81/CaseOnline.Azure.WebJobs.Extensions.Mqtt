using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public interface ICreateMqttConfig
    {
        MqttConfig Create(INameResolver nameResolver, ILogger logger);
    } 
}
