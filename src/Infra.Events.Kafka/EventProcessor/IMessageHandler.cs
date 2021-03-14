using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public interface IMessageHandler<TEvent> where TEvent : Event
    {
        Task Handle(TEvent @event, Dictionary<string, string> headers);
    }
}
