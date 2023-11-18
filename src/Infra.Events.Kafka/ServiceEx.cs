using Autofac;
using Domain;
using Infra.Eevents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;

namespace Infra.Events.Kafka
{
    public static class ServiceEx
    {
        public static void AddKafka(
            this ContainerBuilder builder,
            Action<KafkaProducerConfig> producerConfig,
            Action<SubscriberConfig> subscriberConfig,
            KafkaOptions options = null)
        {
            builder
               .RegisterInstance(Options.Create(options ?? new KafkaOptions()))
               .As<IOptions<KafkaOptions>>()
               .InstancePerLifetimeScope();

            if (producerConfig != null)
            {
                builder.AddKafkaProducer(producerConfig);
            }

            if (subscriberConfig != null)
            {
                builder.AddKafkaConsumer(subscriberConfig);
            }
        }

        public static void AddKafkaProducer(
            this ContainerBuilder builder,
            Action<KafkaProducerConfig> configurator)
        {
            Guard.NotNull(configurator, nameof(configurator));

            // Producer
            builder.RegisterType<KafkaEventBus>().As<IEventBus>().SingleInstance();

            // register producer config
            var config = new KafkaProducerConfig();
            configurator(config);

            builder.RegisterInstance(Options.Create(config));
        }

        public static void AddKafkaConsumer(
            this ContainerBuilder builder,
            Action<SubscriberConfig> configurator)
        {
            Guard.NotNull(configurator, nameof(configurator));

            var config = new SubscriberConfig();

            // Consumer
            configurator(config);

            var events = config.EventAssemblies
                .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsAssignableTo<Event>());

            foreach (var eventType in events)
            {
                if (!eventType.GetConstructors().Any(c => c.GetParameters().Count() == 0))
                {
                    continue;
                }

                var handlerType = typeof(IMessageHandler<>).MakeGenericType(eventType);
                var hasHandler = config
                    .EventAssemblies
                    .Any(ass => ass.GetTypes().Any(ty => handlerType.IsAssignableFrom(ty)));

                if (hasHandler)
                {
                    var topicInfo = eventType.GetCustomAttribute<TopicAttribute>();
                    var topicName = topicInfo?.Name ?? eventType.FullName;

                    if (config.Topics.Contains(topicName))
                        continue;

                    config.Topics.Add(topicName);
                }
            }

            builder.RegisterInstance(Options.Create(config));
            
            builder.RegisterType<KafkaListenerService>()
                .As<IHostedService>()
                .InstancePerDependency();

            builder
                .RegisterAssemblyTypes(config.EventAssemblies)
                .AsClosedTypesOf(typeof(IMessageHandler<>))
                    .AsImplementedInterfaces()
                    .InstancePerDependency();

            builder
                .RegisterType<HandlerInvoker>()
                .SingleInstance();
        }
    }
}
