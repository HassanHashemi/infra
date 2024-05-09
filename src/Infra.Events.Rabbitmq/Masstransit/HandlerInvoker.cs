using System.Collections;
using System.Reflection;
using Autofac;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infra.Events.Rabbitmq.Masstransit;

public class HandlerInvoker
{
	private readonly ILifetimeScope _scope;
	private readonly ILogger<HandlerInvoker> _logger;
	private readonly Assembly[] _scanningAssemblies;

	public HandlerInvoker(
		ILifetimeScope scope,
		ILogger<HandlerInvoker> logger, 
		IOptions<RabbitmqConsumerConfig> options)
	{
		this._scope = scope;
		this._logger = logger;
		this._scanningAssemblies = options.Value.EventAssemblies;
	}
        
	public async Task Invoke(Event @event, Dictionary<string, string> headers)
	{
		var type = GetType(@event.EventName);

		if (type == null)
		{
			_logger.LogWarning($"Could not find handler for {@event.EventName}");
			return;
		}
            
		var handlerType = typeof(Kafka.IMessageHandler<>).MakeGenericType(type);
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