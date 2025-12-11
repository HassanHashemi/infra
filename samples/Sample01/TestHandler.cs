using System.Collections.Generic;
using System.Threading.Tasks;
using Infra.Events.Kafka;

namespace Sample01;

public class TestHandler : IMessageHandler<SampleEvent>
{
    public Task Handle(SampleEvent @event, Dictionary<string, string> headers)
    {
        return Task.CompletedTask;
    }
}