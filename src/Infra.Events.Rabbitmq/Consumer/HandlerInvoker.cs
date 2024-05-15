using System.Collections;
using System.Reflection;
using Autofac;
using Infra.Events.Kafka;
using Infra.Serialization.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infra.Events.Rabbitmq;

public class HandlerInvoker
{
    private readonly ILifetimeScope _scope;
    private readonly ILogger<HandlerInvoker> _logger;
    private readonly Assembly[] _scanningAssemblies;
    private readonly IJsonSerializer _jsonSerializer;

    public HandlerInvoker(
        ILifetimeScope scope,
        ILogger<HandlerInvoker> logger,
        IOptions<RabbitmqConsumerConfig> options)
    {
        this._scope = scope;
        this._logger = logger;
        this._jsonSerializer =  new DefaultNewtonSoftJsonSerializer();
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