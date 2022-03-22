using MQTTnet.Extensions.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;

/// <summary>
/// The configuration for MQTT.
/// </summary>
public class MqttConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MqttConfiguration"/> class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options">The managed options.</param>
    public MqttConfiguration(string name, IManagedMqttClientOptions options)
    {
        Name = name;
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string Name { get; }

    /// <summary>
    /// Gets the managed options.
    /// </summary>
    public IManagedMqttClientOptions Options { get; }

    public override string ToString()
    {
        return $"Name={Name};Client={Options?.ClientOptions?.ClientId}";
    } 
}
