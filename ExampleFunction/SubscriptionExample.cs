using System;
using Microsoft.Azure.WebJobs;
using System.Text;
using Microsoft.Extensions.Logging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using Newtonsoft.Json;

namespace ExampleFunction.Jan
{
    public static class TimerTrigger
    {
        //cmnd/keukenlamp/POWER
        [FunctionName("TimerTrigger")]
        public static void Run(
            [MqttTrigger("schollaartthuis.duckdns.org", 1883, new[] { "owntracks/kees/kees01", "owntracks/marleen/marleen01" }, "kees", "ww")]PublishedMqttMessage message,
            ILogger log,
            [Table("Locations", Connection = "StorageConnectionAppSetting")] out Trail trail)
        {
            var body = Encoding.UTF8.GetString(message.Message);
            log.LogInformation($"Message from topic {message.Topic} body: {body}");

            trail = JsonConvert.DeserializeObject<Trail>(body);
            trail.PartitionKey = message.Topic.Replace("/", "_");
            trail.RowKey = DateTime.Now.Ticks.ToString();
            trail.QosLevel = message.QosLevel.ToString();
            trail.Retain = message.Retain;
        }
    }

    public class Trail : Owntracks
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Topic { get; set; }
        public string QosLevel { get; set; }
        public bool Retain { get; set; }
    }

    public class Owntracks
    {
        public string _type { get; set; }
        public string tid { get; set; }
        public string acc { get; set; }
        public string batt { get; set; }
        public string conn { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string tst { get; set; }
        public string _cp { get; set; }
        public string alt { get; set; }
        public string vac { get; set; }
        public string t { get; set; }

    }
}
