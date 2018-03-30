using System;
using System.Linq;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{

    public class InvalidCustomConfigCreatorException : Exception
    {
        public InvalidCustomConfigCreatorException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
