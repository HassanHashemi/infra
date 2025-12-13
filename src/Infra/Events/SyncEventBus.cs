using Autofac;
using Domain;
using Infra.Eevents;
using Infra.HostedServices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Events;

public class SyncEventBus : IEventBus
{
    private readonly ILifetimeScope _container;
    private readonly IBackgroundTaskInvoker _invoker;

    public SyncEventBus(ILifetimeScope container, IBackgroundTaskInvoker invoker)
    {
        _container = container;
        _invoker = invoker;
    }

    public async Task Execute<TEvent>(TEvent @event, Dictionary<string, string> _ = null, CancellationToken cancellationToken = default) where TEvent : Event
    {
        var exceptions = new List<Exception>();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
        var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);

        if (@event.ForceAsync)
        {
            _invoker.Execute(async rootScope =>
            {
                await using var childScope = rootScope.BeginLifetimeScope();
                dynamic handlers = childScope.ResolveKeyed("1", handlersType);
                foreach (dynamic handlerItem in handlers)
                {
                    try
                    {
                        await handlerItem.HandleEvent((dynamic)@event);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }
        else
        {
            dynamic handlers = _container.ResolveKeyed("1", handlersType);
            foreach (dynamic handlerItem in handlers)
            {
                try
                {
                    await handlerItem.HandleEvent((dynamic)@event);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
        }

        if (exceptions.Count >= 1)
        {
            throw new AggregateException(exceptions);
        }
    }
}