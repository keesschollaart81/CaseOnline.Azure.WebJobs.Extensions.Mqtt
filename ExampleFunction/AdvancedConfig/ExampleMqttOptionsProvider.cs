using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet.ManagedClient;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Protocol;

namespace ExampleFunction.AdvancedConfig
{
    public class ExampleMqttConfigProvider : ICreateMqttConfig
    {
        public MqttConfig Create(INameResolver nameResolver, ILogger logger)
        {
            var options = new ManagedMqttClientOptionsBuilder()
                   .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                   .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                        .WithTcpServer(nameResolver.Resolve("MqttServer"), 1883)
                        .WithCredentials(nameResolver.Resolve("MqttUsername"), nameResolver.Resolve("MqttPassword"))
                        .Build())
                   .Build();

            var topics = new TopicFilter[]{
                new TopicFilter("owntracks/kees/kees01", MqttQualityOfServiceLevel.ExactlyOnce)
            };

            return new MqttConfigExample(nameResolver.Resolve("MqttServer"), options, topics);
        }
    }
}
