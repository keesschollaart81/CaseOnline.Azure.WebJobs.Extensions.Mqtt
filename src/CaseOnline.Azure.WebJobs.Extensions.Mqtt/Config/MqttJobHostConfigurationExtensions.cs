using System;
using Microsoft.Azure.WebJobs;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public static class MqttJobHostConfigurationExtensions
    {
        /// <summary>
        /// Enables use of the Mqtt extensions
        /// </summary>
        /// <param name="config">The <see cref="JobHostConfiguration"/> to configure.</param>
        /// <param name="mqttConfig">The <see cref="MqttExtensionConfigProvider"></see> to use./></param>
        public static void UseMqtt(this JobHostConfiguration config, MqttExtensionConfigProvider mqttConfig = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (mqttConfig == null)
            {
                mqttConfig = new MqttExtensionConfigProvider();
            }

            config.RegisterExtensionConfigProvider(new MqttExtensionConfigProvider());
        } 
    }
}
