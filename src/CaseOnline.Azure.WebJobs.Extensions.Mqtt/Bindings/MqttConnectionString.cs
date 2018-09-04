namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    public class MqttConnectionString
    {
        private const int DetaultMqttPort = 1883;
        private const int DetaultMqttPortWithTls = 8883;
        private const bool DefaultTls = false; 
        private const string KeyForPort = "Port";
        private const string KeyForClientId = "ClientId";
        private const string KeyForServer = "Server";
        private const string KeyForUsername = "Username";
        private const string KeyForPassword = "Password";
        private const string KeyForTls = "Tls";

        private readonly DbConnectionStringBuilder _connectionStringBuilder;

        public MqttConnectionString(string connectionString)
        {
            _connectionStringBuilder = new DbConnectionStringBuilder()
            {
                ConnectionString = connectionString
            };
            ParseAndSetServer();
            ParseAndSetTls();
            ParseAndSetPort();
        }

        public bool Tls { get; private set; }

        public int Port { get; private set; }

        public string Server { get; private set; }

        public string ClientId => _connectionStringBuilder.TryGetValue(KeyForClientId, out var clientIdValue) && !string.IsNullOrEmpty(clientIdValue as string)
                ? clientIdValue.ToString()
                : Guid.NewGuid().ToString();

        public string Username => _connectionStringBuilder.TryGetValue(KeyForUsername, out var userNameValue)
                ? userNameValue.ToString()
                : null;

        public string Password => _connectionStringBuilder.TryGetValue(KeyForPassword, out var passwordValue)
                ? passwordValue.ToString()
                : null;

        private void ParseAndSetServer()
        {
            Server = _connectionStringBuilder.TryGetValue(KeyForServer, out var serverValue)
                ? serverValue.ToString()
                : throw new Exception("No server hostname configured, please set the server via the MqttTriggerAttribute, using the application settings via the Azure Portal or using the local.settings.json");
        }

        private void ParseAndSetTls()
        {
            var tls = DefaultTls;
            var connectionStringHasTls = _connectionStringBuilder.TryGetValue(KeyForTls, out var tlsAsString);
            if (connectionStringHasTls && !string.IsNullOrEmpty(tlsAsString as string))
            {
                var canParseTlsFromConnectionString = bool.TryParse(tlsAsString as string, out tls);
                if (!canParseTlsFromConnectionString)
                {
                    throw new FormatException("Tls has an invalid value");
                }
            }
            Tls = tls;
        }

        private void ParseAndSetPort()
        {
            var port = Tls ? DetaultMqttPortWithTls : DetaultMqttPort;
            var connectionStringHasPort = _connectionStringBuilder.TryGetValue(KeyForPort, out var portAsString);
            if (connectionStringHasPort && !string.IsNullOrEmpty(portAsString as string))
            {
                var canParsePortFromConnectionString = int.TryParse(portAsString as string, out port);
                if (!canParsePortFromConnectionString)
                {
                    throw new FormatException("Port has an invalid value");
                }
            }
            Port = port;
        }

        public override string ToString()
        {
            return $"Server={Server};Port={Port};Username={Username};ClientId={ClientId};Tls={Tls}";
        }
    }
}
