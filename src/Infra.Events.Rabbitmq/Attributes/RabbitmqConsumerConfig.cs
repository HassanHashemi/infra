using System.Reflection;
using Domain;

namespace Infra.Events.Rabbitmq;

public class RabbitmqConsumerConfig
{
	public List<(string queueName, string exchange)> Transports { get; internal set; } = new();
	public Assembly[] EventAssemblies { get; set; }
	public Func<IServiceProvider, Event, Dictionary<string, string>, ValueTask> PreMessageHandlingHandler { get; set; } = null;
    public bool IsValid => EventAssemblies.Any();
}