using Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Eevents
{
    public interface IEventBus
    {
        Task Execute<TEvent>(TEvent @event,
            Dictionary<string, string> headers,
            CancellationToken cancellationToken = default) where TEvent : Event;
    }
}
