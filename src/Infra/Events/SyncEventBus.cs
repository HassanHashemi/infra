using Autofac;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infra.Eevents;
using Domain;

namespace Infra.Events
{
    public class SyncEventBus : IEventBus
    {
        private readonly ILifetimeScope _container;

        public SyncEventBus(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task Execute<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : Event
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
            var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            dynamic handlers = this._container.ResolveKeyed("1", handlersType);

            var exceptions = new List<Exception>();

            foreach (var handlerItem in handlers)
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

            if (exceptions.Count >= 1)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
