using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    public class MqttTriggerBinding : ITriggerBinding
    {
        private readonly ParameterInfo _parameter;
        private readonly IMqttClientFactory _mqttClientFactory;
        private readonly MqttConfiguration _config;
        private readonly ILogger _logger;
        private readonly TraceWriter _traceWriter;
        private readonly IReadOnlyDictionary<string, Type> _emptyBindingContract = new Dictionary<string, Type>();
        private readonly IReadOnlyDictionary<string, object> _emptyBindingData = new Dictionary<string, object>();

        public MqttTriggerBinding(ParameterInfo parameter, IMqttClientFactory mqttClientFactory, MqttConfiguration config, ILogger logger, TraceWriter traceWriter)
        {
            _parameter = parameter;
            _mqttClientFactory = mqttClientFactory;
            _config = config;
            _logger = logger;
            _traceWriter = traceWriter;
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
            _traceWriter.Verbose("MqttTriggerBinding.BindAsync");

            var valueProvider = new ValueProvider(value);
            return Task.FromResult<ITriggerData>(new TriggerData(valueProvider, _emptyBindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            _traceWriter.Verbose("MqttTriggerBinding.CreateListenerAsync");

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<IListener>(new MqttListener(_mqttClientFactory, _config, context.Executor, _logger, _traceWriter));
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

        private class ValueProvider : IValueProvider
        {
            private readonly object _value;

            public ValueProvider(object value)
            {
                _value = value;
            }

            public Type Type
            {
                get { return typeof(PublishedMqttMessage); }
            }

            public Task<object> GetValueAsync()
            {
                return Task.FromResult(_value);
            }

            public string ToInvokeString()
            {
                return string.Empty;
            }
        }
    }
}
