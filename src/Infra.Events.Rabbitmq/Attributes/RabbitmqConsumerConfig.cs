using System.Reflection;
using Domain;

namespace Infra.Events.Rabbitmq;

public class RabbitmqConsumerConfig
{
    public Assembly[] EventAssemblies { get; set; }
    public Func<IServiceProvider, Event, Dictionary<string, string>, ValueTask> PreMessageHandlingHandler { get; set; } = null;

    public bool IsValid => EventAssemblies.Any();
    public ushort PrefetchCount { get; set; }
    public bool GlobalPrefetchCount { get; set; }
}