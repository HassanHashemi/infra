using System.Reflection;
using Domain;

namespace Infra.Events.Rabbitmq;

public class RabbitmqConsumerConfig
{
    public List<(RabbitMqQueue queue, RabbitMqExchange exchange)> Queues { get; internal set; } = new();
    public Assembly[] EventAssemblies { get; set; }
    public Func<IServiceProvider, Event, Dictionary<string, string>, ValueTask> PreMessageHandlingHandler { get; set; } = null;

    public bool IsValid => EventAssemblies.Any();
    public ushort PrefetchCount { get; set; }
    public bool GlobalPrefetchCount { get; set; }
}