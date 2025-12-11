using System.Collections.Generic;
using System.Threading.Tasks;
using Infra.Events.Kafka;
using Orders.Domain.Events;

namespace Sample01;

public class TestHandler2 : IMessageHandler<SampleEvent>
{
    public Task Handle(SampleEvent @event, Dictionary<string, string> headers)
    {
        return Task.CompletedTask;
    }
}