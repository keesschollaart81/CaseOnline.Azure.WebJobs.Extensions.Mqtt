using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host; 
using System.Text;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging; 
using Microsoft.Azure.WebJobs.Host.Triggers;
using System.Reflection;
using System.Threading.Tasks; 
using Microsoft.Azure.WebJobs.Host.Protocols;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.WebJobs.Host.Listeners; 
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Executors;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;

namespace ExampleFunction.Jan
{
    public static class TimerTrigger
    {
        [FunctionName("TimerTrigger")]
        public static void Run([MqttTrigger("192.168.2.2", "cmnd/keukenlamp/POWER", "kees", "ww")]PublishedMqttMessage message, TraceWriter log)
        {
            // create client instance 

        }
    }
     

 
}
