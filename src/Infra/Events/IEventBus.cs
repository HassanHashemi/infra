using Domain;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Eevents
{
    public interface IEventBus
    {
        Task Execute<TEvent>(TEvent @event,
            CancellationToken cancellationToken = default) where TEvent : Event;
    }
}
