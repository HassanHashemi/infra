using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class HandlerInvoker
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger<HandlerInvoker> _logger;
        private readonly IOptions<SubscriberConfig> _options;
        private readonly Assembly[] _scanningAssemblies;
        private static  JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Error = (e, args) => args.ErrorContext.Handled = true,
            ContractResolver = PrivateSetterResolver.Instance,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public HandlerInvoker(
            ILifetimeScope scope,
            ILogger<HandlerInvoker> logger,
            IOptions<SubscriberConfig> options)
        {
            this._scope = scope;
            this._logger = logger;
            this._options = options;
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

            var @event = JsonConvert.DeserializeObject(eventData, type, _settings);

            if (@event == null)
            {
                _logger.LogError($"Could not deserialize to {eventName} payload: {eventData}");

                return;
            }

            var handlerType = typeof(IMessageHandler<>).MakeGenericType(type);
            var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            var handlers = (IEnumerable)_scope.Resolve(handlersType);

            foreach (dynamic handler in handlers)
            {
                await handler.Handle((dynamic)@event, headers);
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