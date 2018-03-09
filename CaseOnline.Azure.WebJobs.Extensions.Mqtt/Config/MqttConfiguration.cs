using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConfiguration : Attribute
    {
        public string ServerUrl { get; set; }
        public string Topic { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }

        public MqttConfiguration(string serverUrl, string topic, string username, string password) : this(serverUrl, topic, username, password, Guid.NewGuid().ToString())
        {
        }

        public MqttConfiguration(string serverUrl, string topic, string username, string password, string clientId)
        {
            ServerUrl = serverUrl;
            Topic = topic;
            Username = username;
            Password = password;
            ClientId = clientId;
        }
    }
}
