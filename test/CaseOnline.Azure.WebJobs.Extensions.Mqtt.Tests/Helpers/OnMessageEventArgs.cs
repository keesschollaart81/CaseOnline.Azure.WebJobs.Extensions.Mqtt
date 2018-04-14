using System;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{
    public class OnMessageEventArgs : EventArgs
    {
        public string ClientId { get; }

        public MqttApplicationMessage ApplicationMessage { get; }

        public OnMessageEventArgs(string clientId, MqttApplicationMessage applicationMessage)
        {
            ClientId = clientId;
            ApplicationMessage = applicationMessage;
        }
    }
}
