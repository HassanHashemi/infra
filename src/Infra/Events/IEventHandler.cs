using Domain;
using System.Threading.Tasks;

namespace Infra.Events;

public interface IEventHandler<in T> where T : Event
{
    /// <summary>
    /// Indicates whether the event handler should execute its work on a background thread (Non-Blocking Async call).
    /// </summary>
    virtual bool RunInBackground => false;

    Task HandleEvent(T @event);
}