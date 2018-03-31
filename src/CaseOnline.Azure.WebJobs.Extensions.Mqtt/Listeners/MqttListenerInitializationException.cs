using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    /// <summary>
    /// Thrown when initialization of MQTT fails.
    /// </summary>
    public class MqttListenerInitializationException : Exception
    { 
        /// <summary>
        /// Intializes a new instance of the <see cref="MqttListenerInitializationException"/> class.
        /// </summary>
        public MqttListenerInitializationException()
        {
        }

        /// <summary>
        /// Intializes a new instance of the <see cref="MqttListenerInitializationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public MqttListenerInitializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Intializes a new instance of the <see cref="MqttListenerInitializationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public MqttListenerInitializationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
