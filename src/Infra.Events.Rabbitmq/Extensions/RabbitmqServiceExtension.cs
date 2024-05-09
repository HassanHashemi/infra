using System.Reflection;
using Autofac;
using Domain;
using Infra.Eevents;

namespace Infra.Events.Rabbitmq;

public static class RabbitmqServiceExtension
{
	public static void AddRabbitmqInternal(
		this ContainerBuilder builder,
		Action<RabbitmqOptions> rabbitmqConfigurator,
		Action<RabbitmqConsumerConfig> consumerConfigurator)
	{
		// Consumer
		Guard.NotNull(consumerConfigurator, nameof(consumerConfigurator));
		var consumerConfig = new RabbitmqConsumerConfig();
		consumerConfigurator(consumerConfig);

		Guard.NotNull(consumerConfigurator, nameof(consumerConfigurator));
		var rabbitmqConfigs = new RabbitmqOptions();
		rabbitmqConfigurator(rabbitmqConfigs);

		var eventInfos = consumerConfig.ExtractAssemblies();

		builder
			.RegisterType<MassTransitEventBus>().As<IEventBus>().SingleInstance();

		builder
			.RegisterAssemblyTypes(consumerConfig.EventAssemblies)
			.AsClosedTypesOf(typeof(Kafka.IMessageHandler<>))
			.AsImplementedInterfaces()
			.InstancePerDependency();

		builder
			.RegisterType<HandlerInvoker>()
			.SingleInstance();
	}

	private static List<RabbitMqTransportInfo> ExtractAssemblies(this RabbitmqConsumerConfig consumerConfig)
	{
		var events = consumerConfig.EventAssemblies
			.SelectMany(a => a.GetTypes())
			.Where(t => t.IsAssignableTo<Event>());

		var eventInfos = new List<RabbitMqTransportInfo>();

		foreach (var eventType in events)
		{
			if (eventType.GetConstructors().All(c => c.GetParameters().Count() != 0))
			{
				continue;
			}

			var handlerType = typeof(Kafka.IMessageHandler<>).MakeGenericType(eventType);
			var hasHandler = consumerConfig
				.EventAssemblies
				.Any(ass => ass.GetTypes().Any(ty => handlerType.IsAssignableFrom(ty)));

			if (hasHandler)
			{
				var queueAttribute = eventType.GetCustomAttribute<QueueAttribute>();
				var rabbitmqExchangeInfo = queueAttribute != null
					? new RabbitMqTransportInfo(queueAttribute.ExchangeName, queueAttribute.QueueName,
						queueAttribute.ExchangeType, queueAttribute.RoutingKey)
					: new RabbitMqTransportInfo(eventType.FullName, eventType.FullName);

				eventInfos.Add(rabbitmqExchangeInfo);
			}
		}

		return eventInfos;
	}
}