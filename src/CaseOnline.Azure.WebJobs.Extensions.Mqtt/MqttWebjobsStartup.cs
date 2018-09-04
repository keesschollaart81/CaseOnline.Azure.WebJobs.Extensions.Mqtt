using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;

[assembly: WebJobsStartup(typeof(MqttWebJobsStartup))]

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    public class MqttWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddMqtt();
        }
    }
}
