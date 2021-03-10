using Autofac;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class HandlerInvoker
    {
        private readonly ILifetimeScope _scope;
        private readonly Assembly[] _scanningAssemblies;

        public HandlerInvoker(ILifetimeScope scope, IOptions<SubscriberConfig> options)
        {
            this._scope = scope;
            this._scanningAssemblies = options.Value.EventAssemblies;
        }

        public async Task Invoke(string eventName, string eventData)
        {
            var type = GetType(eventName);

            if (type == null)
            {
                throw new InvalidOperationException($"Could not find handler for {eventName}");
            }

            var @event = JsonConvert.DeserializeObject(eventData, type);

            if (@event == null)
            {
                throw new InvalidOperationException($"Could not deserialize to {eventName} payload: {eventData}");
            }

            var handlerType = typeof(IMessageHandler<>).MakeGenericType(type);
            var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            var handlers = (IEnumerable)_scope.Resolve(handlersType);

            foreach (dynamic handler in handlers)
            {
                await handler.Handle((dynamic)@event);
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