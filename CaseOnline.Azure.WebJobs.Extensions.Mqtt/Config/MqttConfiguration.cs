using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config
{
    public class MqttConfiguration : Attribute
    {
        public string Server { get; set; }
        public int Port { get; }
        public string[] Topics { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }

        public MqttConfiguration(string server, int port, string[] topics, string username, string password) : this(server, port, topics, username, password, Guid.NewGuid().ToString())
        {
        }

        public MqttConfiguration(string server, int port, string[] topics, string username, string password, string clientId)
        {
            Server = server;
            Port = port;
            Topics = topics;
            Username = username;
            Password = password;
            ClientId = clientId;
        }
    }
}
