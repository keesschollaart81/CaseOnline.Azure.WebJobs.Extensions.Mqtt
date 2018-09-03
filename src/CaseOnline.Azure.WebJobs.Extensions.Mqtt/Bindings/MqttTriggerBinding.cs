using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Listeners;
using CaseOnline.Azure.WebJobs.Extensions.Mqtt.Messaging;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Binds an input parameter to the MQTT queue.
    /// </summary>
    public class MqttTriggerBinding : ITriggerBinding
    {
        private readonly ParameterInfo _parameter;
        private readonly MqttConnection _connection;
        private readonly TopicFilter[] _topics;
        private readonly ILogger _logger;
        private readonly IReadOnlyDictionary<string, object> _emptyBindingData = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttTriggerAttribute"/> class.
        /// </summary>
        /// <param name="parameter">The parameter to bind to.</param>
        /// <param name="connection">The MQTT connection.</param>
        /// <param name="topics">The topics to subscribe to.</param>
        /// <param name="logger">The logger.</param>
        public MqttTriggerBinding(ParameterInfo parameter, MqttConnection connection, TopicFilter[] topics, ILogger logger)
        {
            _parameter = parameter;
            _connection = connection;
            _topics = topics;
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
            var valueProvider = new ValueProvider(value);
            return Task.FromResult<ITriggerData>(new TriggerData(valueProvider, _emptyBindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var listener = new MqttListener(_connection, _topics, context.Executor, _logger);

            _logger.LogDebug("Listener for MqttTriggerBinding created");

            return Task.FromResult<IListener>(listener);
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
