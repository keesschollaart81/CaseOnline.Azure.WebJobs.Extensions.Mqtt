using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{
    public class MqttListenerInitializationException : Exception
    { 
        public MqttListenerInitializationException()
        {
        }

        public MqttListenerInitializationException(string message) : base(message)
        {
        }

        public MqttListenerInitializationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
