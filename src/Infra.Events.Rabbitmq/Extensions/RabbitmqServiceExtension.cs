using System.Reflection;
using Autofac;
using Domain;
using Infra.Eevents;
using Infra.Events.Kafka;
using Infra.Events.Rabbitmq.Rabbitmq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infra.Events.Rabbitmq;

public static class RabbitmqServiceExtension
{
    public static void AddRabbitmqInternal(
        this ContainerBuilder builder,
        Action<RabbitmqOptions> rabbitmqConfigurator,
        Action<RabbitmqConsumerConfig> consumerConfigurator)
    {
        Guard.NotNull(rabbitmqConfigurator, nameof(rabbitmqConfigurator));
        var rabbitmqConfigs = new RabbitmqOptions();
        rabbitmqConfigurator(rabbitmqConfigs);

        // Consumer
        Guard.NotNull(consumerConfigurator, nameof(consumerConfigurator));
        var consumersConfigs = new RabbitmqConsumerConfig();
        consumerConfigurator(consumersConfigs);

        consumersConfigs.ExtractAssemblies();
        builder.RegisterInstance(Options.Create(rabbitmqConfigs));
        builder.RegisterInstance(Options.Create(consumersConfigs));

        builder
            .RegisterAssemblyTypes(consumersConfigs.EventAssemblies)
            .AsClosedTypesOf(typeof(Kafka.IMessageHandler<>))
            .AsImplementedInterfaces()
            .InstancePerDependency();

        builder
            .RegisterType<HandlerInvoker>()
            .SingleInstance();

        builder
            .RegisterType<RabbitMqConsumerService>()
            .SingleInstance();
        
        builder
            .RegisterType<RabbitmqConnectionMultiplexer>()
            .SingleInstance();
        
        builder.RegisterType<RabbitMqStarterHostedService>()
            .As<IHostedService>()
            .InstancePerDependency();

        // Producer
        builder
            .RegisterType<RabbitmqEventBus>().As<IEventBus>().SingleInstance();
    }

    private static void ExtractAssemblies(this RabbitmqConsumerConfig config)
    {
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

            if (!hasHandler)
            {
                continue;
            }

            var queueAttribute = eventType.GetCustomAttribute<QueueAttribute>();
            var queueName = queueAttribute?.QueueName ?? eventType.FullName;
            var exchangeName = queueAttribute?.ExchangeName ?? eventType.FullName;

            if (config.Transports.Any(a => a.queueName == queueName))
            {
                continue;
            }

            config.Transports.Add((queueName, queueAttribute?.ExchangeName ?? exchangeName));
        }
    }
}