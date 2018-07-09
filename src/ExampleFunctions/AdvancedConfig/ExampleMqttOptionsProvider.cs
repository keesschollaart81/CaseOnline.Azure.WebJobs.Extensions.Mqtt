using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.Extensions.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet.Client;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;

namespace ExampleFunction.AdvancedConfig
{
    public class ExampleMqttConfigProvider : ICreateMqttConfig
    {
        public CustomMqttConfig Create(INameResolver nameResolver, ILogger logger)
        {
            var connectionString = new MqttConnectionString(nameResolver.Resolve("MqttConnection"));

            var options = new ManagedMqttClientOptionsBuilder()
                   .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                   .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId(connectionString.ClientId.ToString())
                        .WithTcpServer(connectionString.Server, connectionString.Port)
                        .WithCredentials(connectionString.Username, connectionString.Password)
                        .Build())
                   .Build();
            

            return new MqttConfigExample("CustomConnection", options);
        }
    }
}
