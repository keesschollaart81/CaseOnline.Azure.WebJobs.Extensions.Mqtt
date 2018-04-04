using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Config;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Binds an input parameter to the MQTT queue.
    /// </summary>
    public class MqttTriggerBinding : ITriggerBinding
    {
        private readonly ParameterInfo _parameter;
        private readonly IMqttClientFactory _mqttClientFactory;
        private readonly MqttConfiguration _config;
        private readonly ILogger _logger;
        private readonly IReadOnlyDictionary<string, object> _emptyBindingData = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttTriggerAttribute"/> class.
        /// </summary>
        /// <param name="parameter">The parameter to bind to.</param>
        /// <param name="mqttClientFactory">The MQTT client factory.</param>
        /// <param name="config">The MQTT configuration.</param>
        /// <param name="logger">The logger.</param>
        public MqttTriggerBinding(ParameterInfo parameter, IMqttClientFactory mqttClientFactory, MqttConfiguration config, ILogger logger)
        {
            _parameter = parameter;
            _mqttClientFactory = mqttClientFactory;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Gets the trigger value type.
        /// </summary>
        public Type TriggerValueType => typeof(IMqttMessage);

        /// <summary>
        /// Gets the binding data contract.
        /// </summary>
        public IReadOnlyDictionary<string, Type> BindingDataContract { get; } = new Dictionary<string, Type>();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            _logger.LogDebug("MqttTriggerBinding.BindAsync");

            var valueProvider = new ValueProvider(value);
            return Task.FromResult<ITriggerData>(new TriggerData(valueProvider, _emptyBindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            _logger.LogDebug("MqttTriggerBinding.CreateListenerAsync");

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
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

        private class ValueProvider : IValueProvider
        {
            private readonly object _value;

            public ValueProvider(object value)
            {
                _value = value;
            }

            public Type Type => typeof(IMqttMessage);

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
