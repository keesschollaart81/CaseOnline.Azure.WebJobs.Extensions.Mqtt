using Microsoft.Azure.WebJobs.Description;
using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt
{

    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class MqttTriggerAttribute : Attribute
    {
        public string ServerUrl { get; set; }
        public int Port { get; }
        public string[] Topics { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }

        public MqttTriggerAttribute(string serverUrl, int port, string[] topics, string username, string password) : this(serverUrl, port, topics, username, password, Guid.NewGuid().ToString())
        {
        }

        public MqttTriggerAttribute(string serverUrl, int port, string[] topics, string username, string password, string clientId)
        {
            ServerUrl = serverUrl;
            Port = port;
            Topics = topics;
            Username = username;
            Password = password;
            ClientId = clientId;
        }
    }
}
