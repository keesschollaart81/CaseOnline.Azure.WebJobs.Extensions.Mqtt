using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(MqttWebJobsStartup))]

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt;

public class MqttWebJobsStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddMqtt();
    }
}
