using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{
    public interface IRquireMqttConnection
    {
        string ConnectionString { get; set; }

        Type MqttConfigCreatorType { get; }
    }
}
