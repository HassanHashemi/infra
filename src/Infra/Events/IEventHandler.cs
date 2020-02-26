using Domain;
using System.Threading.Tasks;

namespace Infra.Events
{
    public interface IEventHandler<T> where T : Event
    {
        Task HandleEvent(T @event);
    }
}
