using System.Reflection;
using Autofac;
using Domain;
using Infra.Eevents;
using Infra.Events.Kafka;
using Infra.Events.Rabbitmq.EventProcessor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infra.Events.Rabbitmq;

public static class ServiceExtension
{
    public static void AddRabbitmq(
        this ContainerBuilder builder,
        Action<RabbitmqPublisherConfig> producerConfig,
        Action<RabbitmqConsumerConfig> subscriberConfig,
        RabbitmqOptions options = null)
    {
        builder
            .RegisterInstance(Options.Create(options ?? new RabbitmqOptions()))
            .As<IOptions<RabbitmqOptions>>();

        if (producerConfig != null)
        {
            builder.AddRabbitmqPublisher(producerConfig);
        }

        if (subscriberConfig != null)
        {
            builder.AddRabbitmqConsumer(subscriberConfig);
        }
    }

    public static void AddRabbitmqPublisher(
        this ContainerBuilder builder,
        Action<RabbitmqPublisherConfig> configurator)
    {
        Guard.NotNull(configurator, nameof(configurator));

        // Publisher
        builder.RegisterType<RabbitmqEventBus>().As<IEventBus>().SingleInstance();

        // register producer config
        var config = new RabbitmqPublisherConfig();
        configurator(config);

        builder.RegisterInstance(Options.Create(config));
    }

    public static void AddRabbitmqConsumer(
        this ContainerBuilder builder,
        Action<RabbitmqConsumerConfig> configurator)
    {
        Guard.NotNull(configurator, nameof(configurator));

        var config = new RabbitmqConsumerConfig();

        // Consumer
        configurator(config);

        var events = config.EventAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsAssignableTo<Event>());

        foreach (var eventType in events)
        {
            if (eventType.GetConstructors().All(c => c.GetParameters().Count() != 0))
            {
                continue;
            }

            var handlerType = typeof(IMessageHandler<>).MakeGenericType(eventType);
            var hasHandler = config
                .EventAssemblies
                .Any(ass => ass.GetTypes().Any(ty => handlerType.IsAssignableFrom(ty)));

            if (hasHandler)
            {
                var queueInfo = eventType.GetCustomAttribute<QueueAttribute>();
                var queueName = queueInfo?.Name ?? eventType.FullName;

                if (config.Queues.Contains(queueName))
                    continue;

                config.Queues.Add(queueName);
            }
        }

        builder.RegisterInstance(Options.Create(config));
            
        builder.RegisterType<RabbitmqListenerService>()
            .As<IHostedService>()
            .InstancePerDependency();

        builder
            .RegisterAssemblyTypes(config.EventAssemblies)
            .AsClosedTypesOf(typeof(IMessageHandler<>))
            .AsImplementedInterfaces()
            .InstancePerDependency();

        builder
            .RegisterType<RabbitmqHandlerInvoker>()
            .SingleInstance();
    }
}