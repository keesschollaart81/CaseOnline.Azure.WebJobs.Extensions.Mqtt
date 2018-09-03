using System;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    /// <summary>
    /// Extension methods for MQTT integration.
    /// </summary>
    public static class MqttWebJobsBuilderExtensions
    {
        /// <summary>
        /// Adds the MQTT extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        public static IWebJobsBuilder AddMqtt(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.Services.AddTransient<IMqttClientFactory, MqttFactory>();
            builder.Services.AddTransient<IManagedMqttClientFactory, ManagedMqttClientFactory>();
            builder.Services.AddSingleton<IMqttConnectionFactory, MqttConnectionFactory>();
            builder.AddExtension<MqttExtensionConfigProvider>();

            return builder;
        }
    }
}
