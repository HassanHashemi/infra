using Autofac;
using Infra.Serialization.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Domain;

namespace Infra.Events.Kafka
{
    public class HandlerInvoker
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger<HandlerInvoker> _logger;
        private readonly IOptions<SubscriberConfig> _options;
        private readonly IOptions<KafkaOptions> _kafkaOptions;
        private readonly Assembly[] _scanningAssemblies;
        private readonly IJsonSerializer _jsonSerializer;

        public HandlerInvoker(
            ILifetimeScope scope,
            ILogger<HandlerInvoker> logger,
            IOptions<KafkaOptions> kafkaOptions,
            IOptions<SubscriberConfig> options)
        {
            this._scope = scope;
            this._logger = logger;
            this._kafkaOptions = kafkaOptions;
            this._options = options;
            this._jsonSerializer = _kafkaOptions.Value.Serializer ?? new DefaultNewtonSoftJsonSerializer();
            this._scanningAssemblies = options.Value.EventAssemblies;
        }

        public async Task Invoke(string eventName, string eventData, Dictionary<string, string> headers)
        {
            var type = GetType(eventName);

            if (type == null)
            {
                _logger.LogWarning($"Could not find handler for {eventName}");
                return;
            }

            var @event = _jsonSerializer.Deserialize(eventData, type);
            
            if (@event == null)
            {
                _logger.LogError($"Could not deserialize to {eventName} payload: {eventData}");

                return;
            }

            var handlerType = typeof(IMessageHandler<>).MakeGenericType(type);
            var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            using (var scope = _scope.BeginLifetimeScope())
            {
                var handlers = (IEnumerable) scope.Resolve(handlersType);

                foreach (dynamic handler in handlers)
                {
                    await handler.Handle((dynamic) @event, headers);
                }
            }
        }
        
        public async Task Invoke(Event @event, Dictionary<string, string> headers)
        {
            var type = GetType(@event.EventName);

            if (type == null)
            {
                _logger.LogWarning($"Could not find handler for {@event.EventName}");
                return;
            }
            
            var handlerType = typeof(IMessageHandler<>).MakeGenericType(type);
            var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            await using (var scope = _scope.BeginLifetimeScope())
            {
                var handlers = (IEnumerable) scope.Resolve(handlersType);

                foreach (dynamic handler in handlers)
                {
                    await handler.Handle((dynamic) @event, headers);
                }
            }
        }

        public Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            foreach (var a in _scanningAssemblies)
            {
                type = a.GetType(typeName);

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}