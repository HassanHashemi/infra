using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class KafkaListenerService : BackgroundService
    {
        private bool _consuming = true;

        private readonly ILogger<KafkaListenerService> _logger;
        private readonly SubscriberConfig _config;
        private readonly KafkaListenerCallbacks _callbacks;
        
        public KafkaListenerService(
            ILogger<KafkaListenerService> logger,
            KafkaListenerCallbacks callbacks,
            IOptions<SubscriberConfig> subscriberConfig) : this(logger, callbacks, subscriberConfig.Value)
        { }

        public KafkaListenerService(
            ILogger<KafkaListenerService> logger,
            KafkaListenerCallbacks callbacks,
            SubscriberConfig subscriberConfig)
        {
            if (!subscriberConfig.IsValid)
            {
                throw new ArgumentException(nameof(subscriberConfig));
            }

            this._logger = logger;
            this._config = subscriberConfig;
            this._callbacks = callbacks;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(ConsumerConfig).Build())
            {
                consumer.Subscribe(this._config.Topics);
                while (_consuming)
                {
                    try
                    {
                        var message = consumer.Consume(stoppingToken);
                        await OnMessageReceived(new BusMessageReceivedArgs(message.Topic, message.Message.Value));

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
                    }
                }

                consumer.Close();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        private Task OnMessageReceived(BusMessageReceivedArgs e) => _callbacks.Invoke(this, e);

        private ConsumerConfig ConsumerConfig => new ConsumerConfig
        {
            GroupId = this._config.GroupId,
            BootstrapServers = this._config.BootstrappServers,
            AutoOffsetReset = this._config.OffsetResetType,
            EnableAutoCommit = false
        };
    }
}