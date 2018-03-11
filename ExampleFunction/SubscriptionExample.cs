using System;
using Microsoft.Azure.WebJobs;
using System.Text;
using Microsoft.Extensions.Logging;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;

namespace ExampleFunction.Jan
{
    public static class TimerTrigger
    {
        [FunctionName("TimerTrigger")]
        public static void Run([MqttTrigger("schollaartthuis.duckdns.org", "owntracks/kees/kees01", "kees", "")]PublishedMqttMessage message, ILogger log)
        {
            //cmnd/keukenlamp/POWER
            var body = Encoding.UTF8.GetString(message.Message);
            log.LogInformation($"Message from topic {message.Topic} body: {body}");
            Console.WriteLine($"Message from topic {message.Topic} body: {body}"); 
        }
    }
     

 
}
