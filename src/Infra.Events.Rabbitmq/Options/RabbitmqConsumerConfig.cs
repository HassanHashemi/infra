using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Domain;

namespace Infra.Events.Rabbitmq;

public class RabbitmqConsumerConfig
{
	public List<QueueAttribute> Transports { get; internal set; } = new();
	public Assembly[] EventAssemblies { get; set; }
	public Func<IServiceProvider, Event, Dictionary<string, string>, ValueTask> PreMessageHandlingHandler { get; set; } = null;
    public bool IsValid => EventAssemblies.Any();
	
	/// <summary>
	/// Used as QueueNames prefix
	/// </summary>
    public string ConsumerGroupId { get; set; } = AppDomain.CurrentDomain.FriendlyName;
}