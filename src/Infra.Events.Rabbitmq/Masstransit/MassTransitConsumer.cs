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

    public Task Consume(ConsumeContext<Event> context)
    {
		//if (_config.PreMessageHandlingHandler != null)
		//    await _config.PreMessageHandlingHandler(_serviceProvider, eventData, headers);
        
		return _invoker.Invoke(
            context.Message,
            new Dictionary<string, string>());
    }
}