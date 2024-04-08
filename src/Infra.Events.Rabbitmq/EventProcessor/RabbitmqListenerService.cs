using Infra.Serialization.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Domain;

namespace Infra.Events.Rabbitmq.EventProcessor;

public class RabbitmqListenerService : BackgroundService
{
    private bool _consuming = true;
    private readonly IConnection _connection;
    private readonly IJsonSerializer _serializer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitmqListenerService> _logger;
    private readonly RabbitmqConsumerConfig _consumerConfig;
    private readonly RabbitmqHandlerInvoker _handlerFactory;
    private readonly List<(RabbitMqExchange exchange, RabbitMqQueue queue)> _inProgressConsumers = new();

    public RabbitmqListenerService(
        IConnection connection,
        IServiceProvider serviceProvider,
        ILogger<RabbitmqListenerService> logger,
        RabbitmqOptions rabbitmqOptions,
        RabbitmqConsumerConfig rabbitmqConsumerConfig,
        RabbitmqHandlerInvoker handlerFactory)
    {
        if (!rabbitmqConsumerConfig.IsValid)
        {
            throw new ArgumentException(nameof(rabbitmqConsumerConfig));
        }

        this._logger = logger;
        this._consumerConfig = rabbitmqConsumerConfig;
        this._serviceProvider = serviceProvider;
        this._connection = connection;
        this._handlerFactory = handlerFactory;
        this._serializer = rabbitmqOptions.Serializer ?? new DefaultNewtonSoftJsonSerializer();
    }

    public RabbitmqListenerService(IJsonSerializer serializer)
    {
        _serializer = serializer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeQueues();

        foreach (var inProgressConsumer in _inProgressConsumers)
        {
            _ = Task.Run(() =>
            {
                IModel amqpChannel = default!;

                while (_consuming || !stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        #region //Queue and exchange binding configuration
                        amqpChannel = _connection.CreateModel();
                        amqpChannel.BasicQos(0, _consumerConfig.PrefetchCount, _consumerConfig.GlobalPrefetchCount);

                        if (!string.IsNullOrWhiteSpace(inProgressConsumer.exchange.ExchangeName))
                            amqpChannel.ExchangeDeclare(inProgressConsumer.exchange.ExchangeName, nameof(inProgressConsumer.exchange.ExchangeType).ToLower(), true, false);

                        //if (rabbitMqConsumer.DeadLetterExchangeDetails is not null)
                        //{
                        //    amqpChannel.ExchangeDeclare(rabbitMqConsumer.DeadLetterExchangeDetails);
                        //    rabbitMqConsumer.QueueDetails.QueueArguments.Add(Headers.XDeadLetterExchange,
                        //        rabbitMqConsumer.DeadLetterExchangeDetails.ExchangeName);
                        //}

                        //if (!string.IsNullOrWhiteSpace(queue.RoutingKey))
                        //{
                        //    queue.QueueName = $"{queue.QueueName}.{queue.RoutingKey}";
                        //}

                        var queue = inProgressConsumer.queue;
                        amqpChannel.QueueDeclare(queue.QueueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: new Dictionary<string, object>());

                        amqpChannel.QueueBind(queue.QueueName,
                            exchange: inProgressConsumer.exchange.ExchangeName,
                            routingKey: queue.RoutingKey ?? inProgressConsumer.exchange.RoutingKey,
                            arguments: new Dictionary<string, object>());
                        #endregion

                        #region //Add custom event handler (on message received)
                        var consumer = ActivatorUtilities.CreateInstance<AsyncEventingBasicConsumer>(_serviceProvider, amqpChannel);

                        consumer.Received += async (model, ea) =>
                        {
                            byte[] body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            var eventData = _serializer.Deserialize<Event>(message);

                            var headers =
                                ea.BasicProperties.Headers?
                                    .Select(c => new KeyValuePair<string, string>(c.Key, _serializer.Serialize(c.Value)))
                                    .ToDictionary()
                                ?? new Dictionary<string, string>();

                            if (_consumerConfig.PreMessageHandlingHandler != null)
                            {
                                await _consumerConfig.PreMessageHandlingHandler(_serviceProvider, eventData, headers);
                            }

                            await _handlerFactory.Invoke(
                                eventData.EventName,
                                message,
                                headers);
                        };
                        #endregion

                        #region //Consume messages
                        var result = amqpChannel.BasicConsume(
                            queue: queue.QueueName,
                            autoAck: true,
                            consumer: consumer);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                        _consuming = false;
                        TryDispose(amqpChannel);

                        throw;
                    }
                }

                return Task.CompletedTask;
            }, stoppingToken);
        }
    }

    private static void TryDispose(IModel amqpChannel)
    {
        try
        {
            amqpChannel?.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    private Task InitializeQueues()
    {
        foreach (var queue in _consumerConfig.Queues)
        {
            _inProgressConsumers.Add((queue.exchange, queue.queue));
        }

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _consuming = false;

        return Task.CompletedTask;
    }
}