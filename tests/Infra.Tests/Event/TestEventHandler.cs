using Infra.Events;
using Infra.Events.Kafka;

namespace Infra.Tests.Event;

public class TestEventHandler :
    IEventHandler<TestEvent>,
    IMessageHandler<TestEvent>
{
    public bool RunInBackground => true;

    private readonly EventResultStorage _storage;

    public TestEventHandler(EventResultStorage storage)
    {
        _storage = storage;
    }

    public Task Handle(TestEvent @event, Dictionary<string, string> headers)
    {
        _storage.IntegrationEventResultHasBeenSet++;
        return Task.CompletedTask;
    }

    public Task HandleEvent(TestEvent @event)
    {
        _storage.InternalEventResultHasBeenSet++;
        return Task.CompletedTask;
    }
}