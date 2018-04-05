using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners
{

    public sealed class MqttMessageReceivedEventArgs : EventArgs
    {
        public MqttMessageReceivedEventArgs(IMqttMessage message)
        {
            Message = message;
        }

        public IMqttMessage Message { get; }
    }
}
