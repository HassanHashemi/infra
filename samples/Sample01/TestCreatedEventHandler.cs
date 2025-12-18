using System;
using System.Threading.Tasks;
using Infra.Events;

namespace Sample01;

public class TestCreatedEventHandler : IEventHandler<TestCreatedEvent>
{
    public async Task HandleEvent(TestCreatedEvent @event)
    {
        await Task.Delay(TimeSpan.FromSeconds(60));
        
        //Do nothing
    }
}