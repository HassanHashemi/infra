using Confluent.Kafka;
using Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class KafkaListenerService : BackgroundService
    {
        private bool _consuming = true;

        private readonly ILogger<KafkaListenerService> _logger;
        private readonly SubscriberConfig _config;
        private readonly HandlerInvoker _handlerFactory;

        public KafkaListenerService(
            ILogger<KafkaListenerService> logger,
            HandlerInvoker handlerFactory,
            IOptions<SubscriberConfig> subscriberConfig) : this(logger, handlerFactory, subscriberConfig.Value)
        {
        }

        public KafkaListenerService(
            ILogger<KafkaListenerService> logger,
            HandlerInvoker handlerFactory,
            SubscriberConfig subscriberConfig)
        {
            if (!subscriberConfig.IsValid)
            {
                throw new ArgumentException(nameof(subscriberConfig));
            }

            this._logger = logger;
            this._config = subscriberConfig;
            this._handlerFactory = handlerFactory;

            if (subscriberConfig.Topics == null || !subscriberConfig.Topics.Any())
            {
                _logger.LogWarning("No topics found to subscribe");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(async () =>
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(ConsumerConfig).Build();
                consumer.Subscribe(this._config.Topics);

                while (_consuming)
                {
                    try
                    {
                        var message = consumer.Consume(stoppingToken);
                        var eventData = JsonConvert.DeserializeObject<Event>(message.Message.Value);
                        await _handlerFactory.Invoke(
                            eventData.EventName,
                            message.Message.Value,
                            message.Message.Headers.ToDictionary(
                                k => k.Key,
                                v => Encoding.UTF8.GetString(v.GetValueBytes()))
                            .ToDictionary(d => d.Key, v => v.Value));

                        consumer.Commit(message);

                        _logger.LogInformation($"Consumed Message {message.Message.Value} from topic: {message.Topic}");
                    }
                    catch (OperationCanceledException)
                    {
                        consumer.Close();
                    }
                    catch (Exception e)
                    {
                        consumer.Close();
                        this._logger.LogError(e, e.Message);

                        _consuming = false;
                    }
                }

                consumer.Close();
            });

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consuming = false;

            return Task.CompletedTask;
        }

        private ConsumerConfig ConsumerConfig => new ConsumerConfig
        {
            GroupId = this._config.GroupId,
            BootstrapServers = this._config.BootstrappServers,
            AutoOffsetReset = this._config.OffsetResetType,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };
    }
}