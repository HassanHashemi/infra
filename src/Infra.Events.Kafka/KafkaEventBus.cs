using Confluent.Kafka;
using Domain;
using Infra.Eevents;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
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

            _producer = new ProducerBuilder<Null, string>(new ProducerConfig
            {
                BootstrapServers = config.Value.BootstrapServers
            }).Build();
        }

        public Task Execute(string topic, Event @event, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(topic, nameof(topic));
            Guard.NotNull(@event, nameof(@event));

            var message = new Message<Null, string>
            {
                Value = JsonConvert.SerializeObject(@event)
            };

            AddHeaders(headers, message);

            return _producer.ProduceAsync(topic, message, cancellationToken);
        }

        public Task Execute<TEvent>(TEvent @event, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default) where TEvent : Event
        {
            Guard.NotNull(@event, nameof(@event));

            var eventData = JsonConvert.SerializeObject(@event);
            var message = new Message<Null, string>
            {
                Value = eventData
            };

            AddHeaders(headers, message);

            return _producer.ProduceAsync(@event.EventName, message, cancellationToken);
        }

        private static void AddHeaders(Dictionary<string, string> headers, Message<Null, string> message)
        {
            if (headers != null)
            {
                var headerValues = new Headers();

                foreach (var item in headers)
                {
                    headerValues.Add(item.Key, Encoding.UTF8.GetBytes(item.Value));
                }

                message.Headers = headerValues;
            }
        }
    }
}
