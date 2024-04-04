using Infra.Serialization.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infra.Events.Rabbitmq.EventProcessor;

public class RabbitmqListenerService : BackgroundService
{
    private bool _consuming = true;

    private readonly ILogger<RabbitmqListenerService> _logger;
    private readonly RabbitmqConsumerConfig _config;
    private readonly RabbitmqHandlerInvoker _handlerFactory;
    private readonly RabbitmqOptions _options;
    private readonly IJsonSerializer _serializer;
    private readonly IServiceProvider _serviceProvider;

    public RabbitmqListenerService(
        ILogger<RabbitmqListenerService> logger,
        RabbitmqHandlerInvoker handlerFactory,
        IOptions<RabbitmqConsumerConfig> subscriberConfig,
        IOptions<RabbitmqOptions> options,
        IServiceProvider serviceProvider) : this(logger, handlerFactory, subscriberConfig.Value, options.Value, serviceProvider)
    {
    }

    public RabbitmqListenerService(
        ILogger<RabbitmqListenerService> logger,
        RabbitmqHandlerInvoker handlerFactory,
        RabbitmqConsumerConfig rabbitmqConsumerConfig,
        RabbitmqOptions options,
        IServiceProvider serviceProvider)
    {
        if (!rabbitmqConsumerConfig.IsValid)
        {
            throw new ArgumentException(nameof(rabbitmqConsumerConfig));
        }

        this._logger = logger;
        this._config = rabbitmqConsumerConfig;
        this._serviceProvider = serviceProvider;
        this._handlerFactory = handlerFactory;
        this._options = options;
        this._serializer = options.Serializer ?? new DefaultNewtonSoftJsonSerializer();

        //if (rabbitmqConsumerConfig.Topics == null || !rabbitmqConsumerConfig.Topics.Any())
        //{
        //    _logger.LogWarning("No queues found to subscribe");
        //}
        //else
        //{
        //    _logger.LogInformation($"subscribing to {_serializer.Serialize(rabbitmqConsumerConfig.Topics)}");
        //}
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeTopics();

        _ = Task.Run(async () =>
        {
            //using var consumer = new ConsumerBuilder<Ignore, string>(ConsumerConfig).Build();

            //try
            //{
            //    consumer.Subscribe(this._config.Topics);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.ToString());
            //    throw;
            //}

            //while (_consuming || !stoppingToken.IsCancellationRequested)
            //{
            //    try
            //    {
            //        var message = consumer.Consume(TimeSpan.FromMilliseconds(150));

            //        if (message is null)
            //            continue;

            //        var eventData = _serializer.Deserialize<Event>(message.Message.Value);

            //        var headers = message.Message.Headers.ToDictionary(
            //                k => k.Key,
            //                v => Encoding.UTF8.GetString(v.GetValueBytes()))
            //            .ToDictionary(d => d.Key, v => v.Value);

            //        if (_config.PreMessageHandlingHandler != null)
            //            await _config.PreMessageHandlingHandler(_serviceProvider, eventData, headers);

            //        await _handlerFactory.Invoke(
            //            eventData.EventName,
            //            message.Message.Value,
            //            headers);

            //        consumer.Commit(message);

            //        _logger.LogInformation($"Consumed Message {message.Message.Value} from queue: {message.Topic}");
            //    }
            //    catch (OperationCanceledException ex)
            //    {
            //        _logger.LogError(ex.ToString());
            //        //consumer.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        this._logger.LogError(ex.ToString());
            //        //consumer.Close();

            //        _consuming = false;
            //    }
            //}

            //consumer.Close();
        });
    }

    private async Task InitializeTopics()
    {
        //using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _config.BootstrappServers }).Build();
        //try
        //{
        //    foreach (var queue in _config.Topics)
        //    {
        //        await adminClient.CreateTopicsAsync(new TopicSpecification[]
        //        {
        //            new TopicSpecification
        //            {
        //                Name = queue,
        //                ReplicationFactor = 1,
        //                NumPartitions = 1
        //            }
        //        });
        //    }
        //}
        //catch (CreateTopicsException e)
        //{
        //    _logger.LogError($"An error occured creating queue {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
        //}
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _consuming = false;

        return Task.CompletedTask;
    }
}