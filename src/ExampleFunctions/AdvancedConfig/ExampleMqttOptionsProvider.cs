using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Protocol;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;

namespace ExampleFunction.AdvancedConfig
{
    public class ExampleMqttConfigProvider : ICreateMqttConfig
    {
        public MqttConfig Create(INameResolver nameResolver, ILogger logger)
        {
            var connectionString = new MqttConnectionString(nameResolver.Resolve("MqttConnection"));

            var options = new ManagedMqttClientOptionsBuilder()
                   .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                   .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                        .WithTcpServer(nameResolver.Resolve(connectionString.Server), 1883)
                        .WithCredentials(nameResolver.Resolve(connectionString.Username), nameResolver.Resolve(connectionString.Password))
                        .Build())
                   .Build();

            var topics = new TopicFilter[]{
                new TopicFilter("my/test/topic/#", MqttQualityOfServiceLevel.ExactlyOnce)
            };

            return new MqttConfigExample(options, topics);
        }
    }
}
