using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    public class MqttTriggerBinding : ITriggerBinding
    {
        private readonly ParameterInfo _parameter;
        private readonly MqttTriggerAttribute _mqttTriggerAttribute;
        private readonly MqttConfiguration _config;
        private readonly string _name;
        private TraceWriter _logger;
        private readonly IReadOnlyDictionary<string, Type> _bindingContract;

        public MqttTriggerBinding(ParameterInfo parameter, MqttTriggerAttribute mqttTriggerAttribute, MqttConfiguration config, TraceWriter logger)
        {
            _parameter = parameter;
            _mqttTriggerAttribute = mqttTriggerAttribute;
            _config = config;
            _logger = logger;
            _bindingContract = CreateBindingDataContract();

            var methodInfo = (MethodInfo)parameter.Member;
            _name = string.Format("{0}.{1}", methodInfo.DeclaringType.FullName, methodInfo.Name);
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
            get { return _bindingContract; }
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            _logger.Info("MqttTriggerBinding.BindAsync");
            var mqttInfo = value as PublishedMqttMessage;
            if (mqttInfo == null)
            {
                throw new Exception($"Provided value for {_name} invalid");
            }

            var valueProvider = new ValueProvider(mqttInfo);
            var bindingData = CreateBindingData();

            return Task.FromResult<ITriggerData>(new TriggerData(valueProvider, bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            _logger.Info("MqttTriggerBinding.CreateListenerAsync");
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return Task.FromResult<IListener>(new MqttListener(_config, context.Executor, _logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            MqttTriggerParameterDescriptor descriptor = new MqttTriggerParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "Mqtt executed on schedule"
                }
            };
            return descriptor;
        }

        private IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            Dictionary<string, Type> contract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            contract.Add("MqttTrigger", typeof(string));

            return contract;
        }

        private IReadOnlyDictionary<string, object> CreateBindingData()
        {
            Dictionary<string, object> bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            bindingData.Add("MqttTrigger", DateTime.Now.ToString());

            return bindingData;
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

        private class MqttTriggerParameterDescriptor : TriggerParameterDescriptor
        {
            public override string GetTriggerReason(IDictionary<string, string> arguments)
            {
                return string.Format("Mqtt fired at {0}", DateTime.Now.ToString("o"));
            }
        }
    }
}
