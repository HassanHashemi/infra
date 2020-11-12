using Infra.Eevents;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infra.Events.Kafka
{
    public static class ServiceEx
    {
        public static void AddMessageHandler<T>(this IServiceCollection services)
            where T : class, IMessageHandler
        {
            services.AddHostedService<T>();
        }

        public static void AddKafka(this IServiceCollection services,
            Action<KafkaProducerConfig> producerConfig,
            Action<SubscriberConfig> subscriberConfig)
        {
            if (producerConfig != null)
            {
                services.AddKafkaProducer(producerConfig);
            }

            if (subscriberConfig != null)
            {
                services.AddKafkaConsumer(subscriberConfig);
            }
        }

        public static void AddKafkaProducer(this IServiceCollection services, Action<KafkaProducerConfig> configurator)
        {
            Guard.NotNull(configurator, nameof(configurator));

            // Producer
            services.AddSingleton<IEventBus, KafkaEventBus>();
            services.Configure(configurator);
        }

        public static void AddKafkaConsumer(this IServiceCollection services, Action<SubscriberConfig> configurator)
        {
            Guard.NotNull(configurator, nameof(configurator));

            // Consumer
            services.Configure(configurator);

            services.AddSingleton<KafkaListener>();
            services.AddHostedService<KafkaListenerService>();
            services.AddSingleton(new KafkaListenerCallbacks());
        }
    }
}
