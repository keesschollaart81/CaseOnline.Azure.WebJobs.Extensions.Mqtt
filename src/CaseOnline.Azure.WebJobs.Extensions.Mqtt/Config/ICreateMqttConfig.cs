using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    /// <summary>
    /// Allows custom configuration to be used for the MQTT binding.
    /// </summary>
    public interface ICreateMqttConfig
    {
        /// <summary>
        /// Creates the <see cref="MqttConfiguration"/>.
        /// </summary>
        /// <param name="nameResolver">The name resolver.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The MQTT configuration.</returns>
        MqttConfig Create(INameResolver nameResolver, ILogger logger);
    } 
}
