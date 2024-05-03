using Infra.Events.Kafka;
using MassTransit;
using Event = Domain.Event;

namespace Infra.Events.Rabbitmq;

public class MassTransitConsumer : IConsumer<Event>
{
    private readonly HandlerInvoker _invoker;
    public MassTransitConsumer(HandlerInvoker invoker)
    {
        _invoker = invoker;
    }

    public Task Consume(ConsumeContext<Event> @event)
    {
        if (@event is null)
            return Task.CompletedTask;

        //if (_config.PreMessageHandlingHandler != null)
        //    await _config.PreMessageHandlingHandler(_serviceProvider, eventData, headers);

        return _invoker.Invoke(
            @event as Event,
            new Dictionary<string, string>());
    }
}