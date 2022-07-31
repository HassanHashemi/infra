using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class PrivateResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            //TODO: Maybe cache
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }

    public class HandlerInvoker
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger<HandlerInvoker> _logger;
        private readonly IOptions<SubscriberConfig> _options;
        private readonly Assembly[] _scanningAssemblies;
        private static  JsonSerializerSettings _settings;

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
                throw new InvalidOperationException($"Could not find handler for {eventName}");
            }

            _settings = new JsonSerializerSettings
            {
                Error = (e, args) => args.ErrorContext.Handled = true,
                ContractResolver = new PrivateResolver(),
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
            
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