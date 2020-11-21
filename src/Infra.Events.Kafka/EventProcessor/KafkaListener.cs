using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class KafkaListener
    {
        private bool _consuming = true;

        private readonly ILogger<KafkaListenerService> _logger;
        private readonly SubscriberConfig _config;
        private readonly KafkaListenerCallbacks _callbacks;
        
        public KafkaListener(
            ILogger<KafkaListenerService> logger,
            KafkaListenerCallbacks callbacks,
            IOptions<SubscriberConfig> subscriberConfig) : this(logger, callbacks, subscriberConfig.Value)
        { }

        public KafkaListener(
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

        public Task Start(CancellationToken cancellationToken)
        {
            return Task.Run(() => RunListener(cancellationToken));
        }

        private async Task RunListener(CancellationToken cancellationToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(ConsumerConfig).Build())
            {
                Console.WriteLine("before subscribe");
                consumer.Subscribe(this._config.Topics);
                Console.WriteLine("after sub");
                while (_consuming)
                {
                    try
                    {
                        var message = consumer.Consume(cancellationToken);
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _consuming = false;
            return Task.CompletedTask;
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