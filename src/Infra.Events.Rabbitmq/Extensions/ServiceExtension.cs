using System.Reflection;
using Autofac;
using Domain;
using Infra.Eevents;
using Infra.Events.Kafka;
using Infra.Events.Rabbitmq.EventProcessor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infra.Events.Rabbitmq;

public static class ServiceExtension
{
    public static void AddRabbitmq(
        this ContainerBuilder builder,
        Action<RabbitmqOptions> rabbitmqOptions,
        Action<RabbitmqPublisherConfig> publisherConfig,
        Action<RabbitmqConsumerConfig> consumerConfig,
        RabbitmqOptions options = null)
    {
        builder
            .RegisterInstance(Options.Create(options ?? new RabbitmqOptions()))
            .As<IOptions<RabbitmqOptions>>();

        builder.AddRabbitmqInternal(rabbitmqOptions);

        if (publisherConfig != null)
        {
            builder.AddRabbitmqPublisher(rabbitmqOptions, publisherConfig);
        }

        if (consumerConfig != null)
        {
            builder.AddRabbitmqConsumer(rabbitmqOptions, consumerConfig);
        }
    }

    public static void AddRabbitmqPublisher(
        this ContainerBuilder builder,
        Action<RabbitmqOptions> rabbitmqOptions,
        Action<RabbitmqPublisherConfig> configurator)
    {
        builder.AddRabbitmqInternal(rabbitmqOptions);

        builder.RegisterType<RabbitmqEventBus>().As<IEventBus>().SingleInstance();

        // Publisher
        Guard.NotNull(configurator, nameof(configurator));
        var config = new RabbitmqPublisherConfig();
        configurator(config);

        builder.RegisterInstance(Options.Create(config));
    }

    public static void AddRabbitmqConsumer(
        this ContainerBuilder builder,
        Action<RabbitmqOptions> rabbitmqOptions,
        Action<RabbitmqConsumerConfig> configurator)
    {
        AddRabbitmqInternal(builder, rabbitmqOptions);

        // Consumer
        Guard.NotNull(configurator, nameof(configurator));
        var config = new RabbitmqConsumerConfig();
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
                RabbitMqExchange rabbitmqExchangeInfo = new RabbitMqExchange();
                var exchangeInfo = eventType.GetCustomAttribute<ExchangeAttribute>();
                if (exchangeInfo != null)
                {
                    rabbitmqExchangeInfo = new RabbitMqExchange(exchangeInfo.Name, exchangeInfo.ExchangeType, exchangeInfo.RoutingKey);
                }

                var queueInfo = eventType.GetCustomAttribute<QueueAttribute>();
                if (queueInfo != null)
                {
                    var rabbitmqQueue = new RabbitMqQueue(queueInfo.Name, queueInfo.RoutingKey);

                    config.Queues.Add(
                        new ValueTuple<RabbitMqQueue, RabbitMqExchange>(rabbitmqQueue, rabbitmqExchangeInfo));
                }
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

    private static void AddRabbitmqInternal(
        this ContainerBuilder builder,
        Action<RabbitmqOptions> rabbitmqOptions)
    {
        var config = new RabbitmqOptions();
        rabbitmqOptions(config);

        var connectionFactory = new ConnectionFactory
        {
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            UserName = config.UserName,
            Password = config.Password,
            VirtualHost = config.VirtualHost ?? ConnectionFactory.DefaultVHost,
            HostName = config.HostName,
            Port = config.Port,
            ClientProvidedName = config.ClientProvidedName,
            RequestedHeartbeat = TimeSpan.FromSeconds(20),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            ClientProperties = new Dictionary<string, object>
            {
                {
                    "Connection", 0
                }
            }
        };

        var amqpConnection = connectionFactory.CreateConnection();

        builder.RegisterInstance(amqpConnection).As<IConnection>().SingleInstance();
    }
}