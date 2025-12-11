using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Infra.Serialization.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infra.Events.Rabbitmq;

public class RabbitMqConsumerService : IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly IJsonSerializer _serializer;
    private readonly HandlerInvoker _handlerFactory;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly RabbitmqConsumerConfig _consumerConfig;

    public RabbitMqConsumerService(
        HandlerInvoker handlerFactory,
        ILogger<RabbitMqConsumerService> logger,
        IOptions<RabbitmqOptions> options,
        IOptions<RabbitmqConsumerConfig> consumerConfig,
        RabbitmqConnectionMultiplexer connectionMultiplexer) :
        this(logger, handlerFactory, options.Value, consumerConfig.Value, connectionMultiplexer)
    {
    }

    public RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        HandlerInvoker handlerFactory,
        RabbitmqOptions rabbitmqOptions,
        RabbitmqConsumerConfig consumerConfig,
        RabbitmqConnectionMultiplexer connectionMultiplexer)
    {
        if (!consumerConfig.IsValid)
        {
            throw new ArgumentException(nameof(consumerConfig));
        }

        this._logger = logger;
        this._consumerConfig = consumerConfig;
        this._handlerFactory = handlerFactory;
        this._connection = connectionMultiplexer.GetConnection();
        this._channel = _connection.CreateModel();
        this._serializer = rabbitmqOptions.Serializer ?? new DefaultNewtonSoftJsonSerializer();

        if (consumerConfig.Transports == null || !consumerConfig.Transports.Any())
        {
            _logger.LogWarning("No queues found for consuming");
        }
        else
        {
            _logger.LogInformation($"Consuming on queue" + string.Join("\n",
                consumerConfig.Transports.Select(x =>
                    $"Queue:{_consumerConfig.ConsumerGroupId}.{x.QueueName} ,Exchange:{x.ExchangeName}")));
        }
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            foreach (var assembly in _consumerConfig.Transports)
            {
                //Each microservice should have their own queues
                var queueName = $"{_consumerConfig.ConsumerGroupId}.{assembly.QueueName}";

                _channel.ExchangeDeclare(assembly.ExchangeName, assembly.ExchangeType.ToString().ToLower(), durable: true, autoDelete: false);

                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, null);

                _channel.QueueBind(queueName, assembly.ExchangeName, assembly.RoutingKey);


                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += (_, eventArgs) =>
                {
                    Receive(eventArgs.Body, _channel, eventArgs.DeliveryTag, eventArgs.BasicProperties);
                    return Task.CompletedTask;
                };
                _channel.BasicConsume(queueName, autoAck: false, consumer);
                await Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }

    private async void Receive(ReadOnlyMemory<byte> eventArgs, IModel channel, ulong eventArgsDeliveryTag, IBasicProperties properties)
    {
        try
        {
            var payloadString = Encoding.UTF8.GetString(eventArgs.ToArray());

            var @event = _serializer.Deserialize<Event>(payloadString);

            var headers = properties.Headers?.ToDictionary(x => x.Key, y => (string)y.Value);

            //Invoke event handler
            await _handlerFactory.Invoke(@event.EventName ?? properties.Type, payloadString, headers);

            //Send ACK to channel
            channel.BasicAck(eventArgsDeliveryTag, multiple: false);

            _logger.LogInformation("Consumed message from Queue: {Queue} ,payload: {Payload}",
                $"{_consumerConfig.ConsumerGroupId}.{@event.EventName}", payloadString);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }

    public void Dispose()
    {
        if (_channel.IsOpen)
            _channel.Close();

        if (_connection.IsOpen)
            _connection.Close();
    }
}