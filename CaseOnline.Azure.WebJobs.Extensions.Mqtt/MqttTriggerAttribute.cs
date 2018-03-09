using Microsoft.Azure.WebJobs.Description;
using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{

    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class MqttTriggerAttribute : Attribute
    {
        public string ServerUrl { get; set; }
        public string Topic { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }

        public MqttTriggerAttribute(string serverUrl, string topic, string username, string password) : this(serverUrl, topic, username, password, Guid.NewGuid().ToString())
        {
        }

        public MqttTriggerAttribute(string serverUrl, string topic, string username, string password, string clientId)
        {
            ServerUrl = serverUrl;
            Topic = topic;
            Username = username;
            Password = password;
            ClientId = clientId;
        }
    }
}
