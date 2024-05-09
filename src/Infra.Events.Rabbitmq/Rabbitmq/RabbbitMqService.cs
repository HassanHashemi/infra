using System.Text;
using Domain;
using Infra.Serialization.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infra.Events.Rabbitmq;

public class RabbitMqService
{
    private EventingBasicConsumer _consumer;

    private readonly IConnection _connection;
    private readonly IJsonSerializer _serializer;
    private readonly HandlerInvoker _handlerFactory;
    private readonly RabbitmqConsumerConfig _consumerConfig;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(
        HandlerInvoker handlerFactory,
        ILogger<RabbitMqService> logger,
        IOptions<RabbitmqOptions> rabbitmqOptions,
        IOptions<RabbitmqConsumerConfig> consumerConfig) :
        this(logger, handlerFactory, rabbitmqOptions.Value, consumerConfig.Value)
    {
    }

    public RabbitMqService(
        ILogger<RabbitMqService> logger,
        HandlerInvoker handlerFactory,
        RabbitmqOptions rabbitmqOptions,
        RabbitmqConsumerConfig consumerConfig)
    {
        if (!consumerConfig.IsValid)
        {
            throw new ArgumentException(nameof(consumerConfig));
        }

        this._logger = logger;
        this._consumerConfig = consumerConfig;
        this._handlerFactory = handlerFactory;
        this._serializer = rabbitmqOptions.Serializer ?? new DefaultNewtonSoftJsonSerializer();

        //Initialize rabbitmq connection
        {
            var rabbitmqConnectionFactory = new ConnectionFactory
            {
                HostName = rabbitmqOptions.Host,
                UserName = rabbitmqOptions.UserName,
                Password = rabbitmqOptions.Password,
                VirtualHost = rabbitmqOptions.VirtualHost
            };

            this._connection = rabbitmqConnectionFactory.CreateConnection();

            _logger.LogInformation($"Starting rabbitmq connection on host:{rabbitmqOptions.Host}");
        }

        if (consumerConfig.Transports == null || !consumerConfig.Transports.Any())
            _logger.LogWarning("No queues found for consuming");
        else
            _logger.LogInformation($"Consuming on queue {_serializer.Serialize(consumerConfig.Transports)}");
    }

    internal IConnection GetConnection() => this._connection;

    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            foreach (var assembly in _consumerConfig.Transports)
            {
                using (_connection)
                {
                    using (var channel = _connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: assembly.exchange, type: ExchangeType.Fanout);

                        channel.QueueBind(queue: assembly.queueName,
                            exchange: assembly.exchange,
                            routingKey: "");

                        this._consumer = new EventingBasicConsumer(channel);

                        this._consumer.Received += (model, eventArgs) =>
                        {
                            Receive(eventArgs.Body);
                        };

                        channel.BasicConsume(queue: assembly.queueName,
                            autoAck: true,
                            consumer: _consumer);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }

        return Task.CompletedTask;
    }

    private async void Receive(ReadOnlyMemory<byte> eventArgs)
    {
        try
        {
            var payloadString = Encoding.UTF8.GetString(eventArgs.ToArray());

            var @event = _serializer.Deserialize<Event>(payloadString);

            _logger.LogInformation("Consumed message from Queue: {Queue} ,payload: {Payload}", @event.EventName, payloadString);

            await _handlerFactory.Invoke(@event.EventName, payloadString, new Dictionary<string, string>());
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }
}