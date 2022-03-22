﻿using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Extensions.DependencyInjection;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;

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
        builder.Services.AddTransient<IMqttFactory>(_ => new MqttFactory()); 
        builder.Services.AddTransient<IManagedMqttClientFactory, ManagedMqttClientFactory>();
        builder.Services.AddSingleton<IMqttConnectionFactory, MqttConnectionFactory>();
        builder.Services.AddTransient<IMqttConfigurationParser, MqttConfigurationParser>();
        builder.AddExtension<MqttExtensionConfigProvider>();

        return builder;
    }
}
