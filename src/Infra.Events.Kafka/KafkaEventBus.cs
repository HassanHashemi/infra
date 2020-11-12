using Confluent.Kafka;
using Domain;
using Infra.Eevents;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class KafkaEventBus : IEventBus
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaEventBus(KafkaProducerConfig config) : this(Options.Create(config))
        {
        }

        public KafkaEventBus(IOptions<KafkaProducerConfig> config)
        {
            Guard.NotNull(config.Value, nameof(config));

            _producer = new ProducerBuilder<Null, string>(new ProducerConfig()
            {
                BootstrapServers = config.Value.BootstrapServers
            }).Build();
        }

        public Task Execute<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : Event
        {
            Guard.NotNull(@event, nameof(@event));

            var message = new Message<Null, string>()
            {
                Value = JsonConvert.SerializeObject(@event)
            };

            return _producer.ProduceAsync(@event.EventName, message, cancellationToken);
        }
    }
}
