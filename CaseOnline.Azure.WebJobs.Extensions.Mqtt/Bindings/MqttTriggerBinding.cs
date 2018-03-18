using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    public class MqttTriggerBinding : ITriggerBinding
    {
        private readonly ParameterInfo _parameter;
        private readonly IMqttClientFactory _mqttClientFactory;
        private readonly MqttConfiguration _config; 
        private TraceWriter _logger;
        private readonly IReadOnlyDictionary<string, Type> _emptyBindingContract = new Dictionary<string, Type>();
        private readonly IReadOnlyDictionary<string, object> _emptyBindingData = new Dictionary<string, object>();

        public MqttTriggerBinding(ParameterInfo parameter, IMqttClientFactory mqttClientFactory, MqttConfiguration config, TraceWriter logger)
        {
            _parameter = parameter;
            _mqttClientFactory = mqttClientFactory;
            _config = config;
            _logger = logger; 
        }

        public Type TriggerValueType
        {
            get
            {
                return typeof(PublishedMqttMessage);
            }
        }

        public IReadOnlyDictionary<string, Type> BindingDataContract
        {
            get { return _emptyBindingContract; }
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            _logger.Info("MqttTriggerBinding.BindAsync");
            return Task.FromResult<ITriggerData>(new TriggerData(null, _emptyBindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            _logger.Info("MqttTriggerBinding.CreateListenerAsync");
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
           
            return Task.FromResult<IListener>(new MqttListener(_mqttClientFactory, _config, context.Executor, _logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            MqttTriggerParameterDescriptor descriptor = new MqttTriggerParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "Mqtt executed"
                }
            };
            return descriptor;
        }

        private class MqttTriggerParameterDescriptor : TriggerParameterDescriptor
        {
            public override string GetTriggerReason(IDictionary<string, string> arguments)
            {
                return string.Format("Mqtt fired at {0}", DateTime.Now.ToString("o"));
            }
        }
    }
}
