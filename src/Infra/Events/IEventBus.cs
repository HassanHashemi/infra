using Infra.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Eevents
{
    public interface IEventBus
    {
        Task Execute<TEvent>(TEvent @event,
            CancellationToken cancellationToken = default(CancellationToken)) where TEvent : Event;
    }
}
