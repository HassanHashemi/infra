using System.Collections.Generic;
using System.Threading.Tasks;
using Infra.Events.Kafka;
using Sample;

namespace ConsumerSample.EventHandlers;

public class TestHandler : IMessageHandler<FlightOrderItemStateChanged>
{
    public Task Handle(FlightOrderItemStateChanged @event, Dictionary<string, string> headers)
    {
        return Task.CompletedTask;
    }
}