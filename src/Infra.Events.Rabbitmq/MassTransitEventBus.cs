using Infra.Eevents;
using MassTransit;
using Event = Domain.Event;

namespace Infra.Events.Rabbitmq;

public class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task Execute<TEvent>(TEvent @event, Dictionary<string, string> headers, CancellationToken cancellationToken = default) 
        where TEvent : Event
    {
        return _publishEndpoint.Publish(@event, cancellationToken);
    }
}