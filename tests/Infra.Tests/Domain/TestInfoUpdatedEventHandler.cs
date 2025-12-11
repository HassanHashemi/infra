using Infra.Events;

namespace Infra.Tests.Domain;

public class TestInfoUpdatedEventHandler : IEventHandler<TestInfoUpdatedEvent>
{
    public async Task HandleEvent(TestInfoUpdatedEvent @event)
    {
        await Task.Delay(TimeSpan.FromSeconds(20));

        //Do nothing
    }
}