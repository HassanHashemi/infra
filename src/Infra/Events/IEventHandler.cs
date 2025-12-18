using Domain;
using System.Threading.Tasks;

namespace Infra.Events;

public interface IEventHandler<in T> where T : Event
{
    Task HandleEvent(T @event);
}