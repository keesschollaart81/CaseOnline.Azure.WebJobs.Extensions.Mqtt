using Xunit;
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
