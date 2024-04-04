using System.Reflection;
using Domain;

namespace Infra.Events.Rabbitmq;

public class RabbitmqConsumerConfig
{
    public List<string> Queues { get; internal set; } = new List<string>();
    public Assembly[] EventAssemblies { get; set; }
    public Func<IServiceProvider, Event, Dictionary<string, string>, ValueTask> PreMessageHandlingHandler { get; set; } = null;

    public bool IsValid => EventAssemblies.Any();
}