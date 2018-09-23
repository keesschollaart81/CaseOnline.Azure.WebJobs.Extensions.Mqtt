using System;
using System.Runtime.Serialization;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    /// <summary>
    /// Thrown when initialization of MQTT fails.
    /// </summary>
    [Serializable]
    public class MqttConnectionException : Exception
    { 
        /// <summary>
        /// Intializes a new instance of the <see cref="MqttConnectionException"/> class.
        /// </summary>
        public MqttConnectionException()
        {
        }

        /// <summary>
        /// Intializes a new instance of the <see cref="MqttConnectionException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public MqttConnectionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Intializes a new instance of the <see cref="MqttConnectionException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public MqttConnectionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MqttConnectionException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
