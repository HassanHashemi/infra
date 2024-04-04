using System.Reflection;
using System.Text;
using Domain;
using Infra.Eevents;
using Infra.Serialization.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infra.Events.Rabbitmq;

public class RabbitmqEventBus : IEventBus
{
    private IModel _channel;
    private readonly IConnection _connection;
    private readonly IJsonSerializer _serializer;
    private readonly ILogger<RabbitmqEventBus> _logger;
    private readonly RabbitmqPublisherConfig _publisherConfig;

    public RabbitmqEventBus(
        IEnumerable<IConnection> connection,
        IOptions<RabbitmqPublisherConfig> config,
        ILogger<RabbitmqEventBus> logger,
        IOptions<RabbitmqOptions> options)
    {
        this._connection = connection.GetAmqpConnection();

        Guard.NotNull(config.Value, nameof(config));
        _publisherConfig = config.Value;

        _logger = logger;
        _serializer = options.Value.Serializer ?? new DefaultNewtonSoftJsonSerializer();
    }

    private IModel InitChannel(IModel amqpChannel, string exchangeName = default,
        ExchangeType exchangeType = default, Dictionary<string, object> headers = default)
    {
        if (amqpChannel is not null && amqpChannel.IsOpen)
        {
            return amqpChannel;
        }
        else
        {
            amqpChannel = _connection.CreateModel();

            if (!string.IsNullOrWhiteSpace(exchangeName))
            {
                var exchangeTypeValue = exchangeType == default
                    ? nameof(ExchangeType.Direct).ToLower()
                    : nameof(exchangeType).ToLower();

                headers ??= new Dictionary<string, object>();

                amqpChannel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: exchangeTypeValue,
                    durable: true,
                    autoDelete: false,
                    arguments: headers.Select((a, _) => new KeyValuePair<string, object>(a.Key, a.Value)).ToDictionary());
            }

            return amqpChannel;
        }
    }

    public Task Execute(string queue, Event @event, Dictionary<string, string> headers = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(@event, nameof(@event));

        var exchangeInfo = GetExchangeInfo(@event);
        var queueName = GetQueueName(@event);

        InitChannel(_channel, exchangeInfo.name, exchangeInfo.type,
            headers?.Select((a, _) => new KeyValuePair<string, object>(a.Key, a.Value)).ToDictionary());

        var properties = _channel.CreateBasicProperties();

        _channel.BasicPublish(
            exchangeInfo.name,
            routingKey: queueName,
            body: SerializeToRabbitMqMessage(@event),
            basicProperties: properties);

        return Task.CompletedTask;
    }

    public Task Execute<TEvent>(TEvent @event, Dictionary<string, string> headers,
        CancellationToken cancellationToken = default) where TEvent : Event
    {
        Guard.NotNull(@event, nameof(@event));

        var exchangeInfo = GetExchangeInfo(@event);
        var queueName = GetQueueName(@event);

        InitChannel(_channel, exchangeInfo.name, exchangeInfo.type,
            headers.Select((a, _) => new KeyValuePair<string, object>(a.Key, a.Value)).ToDictionary());

        var properties = _channel.CreateBasicProperties();

        _channel.BasicPublish(
            exchangeInfo.name,
            routingKey: queueName,
            body: SerializeToRabbitMqMessage(@event),
            basicProperties: properties);

        return Task.CompletedTask;
    }

    private (string name, ExchangeType type) GetExchangeInfo<TEvent>(TEvent @event) where TEvent : Event
    {
        var exhangeInfo = @event.GetType()
            .GetCustomAttribute<ExchangeAttribute>();

        if (exhangeInfo != null)
            return (exhangeInfo.Name, exhangeInfo.ExchangeType);

        return (default, ExchangeType.Direct);
    }

    private string GetQueueName<TEvent>(TEvent @event) where TEvent : Event
    {
        var exhangeInfo = @event.GetType()
            .GetCustomAttribute<QueueAttribute>();

        if (exhangeInfo != null)
            return exhangeInfo.Name;

        return default;
    }

    public byte[] SerializeToRabbitMqMessage<TObject>(TObject obj)
    {
        if (obj is null)
            return default;

        var json = _serializer.Serialize(obj);
        var finalBody = Encoding.UTF8.GetBytes(json);
        return finalBody;
    }
}