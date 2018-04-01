using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;
using Xunit;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings;
using System.Linq;
using System;
using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Util.Helpers;
using System.Text;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class PublishedMqttMessageTests
    {
        [Fact]
        public void PublishedMqttMessageIsConstructedWell()
        {
            // Arrange 
            var bodyBytes = Encoding.UTF8.GetBytes("{ \"test\": \"case\" }");
            
            // Act
            var message = new PublishedMqttMessage("test/topic", bodyBytes, "qosLevel", false);
            var returnBody = message.GetMessage();

            // Assert  
            Assert.Equal(bodyBytes, returnBody);
            Assert.Equal("test/topic", message.Topic);
        }
    }
}
