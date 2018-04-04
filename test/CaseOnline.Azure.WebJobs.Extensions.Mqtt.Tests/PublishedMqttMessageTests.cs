using Xunit;
using System.Text;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class MqttMessageTests
    {
        [Fact]
        public void MqttMessageIsConstructedWell()
        {
            // Arrange 
            var bodyBytes = Encoding.UTF8.GetBytes("{ \"test\": \"case\" }");
            
            // Act
            var message = new MqttMessage("test/topic", bodyBytes, MqttQualityOfServiceLevel.AtLeastOnce, false);
            var returnBody = message.GetMessage();

            // Assert  
            Assert.Equal(bodyBytes, returnBody);
            Assert.Equal("test/topic", message.Topic);
            Assert.Equal(MqttQualityOfServiceLevel.AtLeastOnce, message.QosLevel);
        }
    }
}
