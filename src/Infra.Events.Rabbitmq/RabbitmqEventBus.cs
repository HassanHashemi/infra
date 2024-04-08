using System.Reflection;
using System.Text;
using Domain;
using Infra.Eevents;
using Infra.Events.Rabbitmq.Extensions;
using Infra.Serialization.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infra.Events.Rabbitmq;

public class RabbitmqEventBus : IEventBus
{
    private readonly IConnection _connection;
    private readonly IJsonSerializer _serializer;
    private readonly ILogger<RabbitmqEventBus> _logger;
    private readonly RabbitmqPublisherConfig _publisherConfig;
    private readonly List<(IModel channel, RabbitMqExchange exchange)> _amqpChannels = new();

    public RabbitmqEventBus(
        IEnumerable<IConnection> connection,
        IOptions<RabbitmqPublisherConfig> config,
        ILogger<RabbitmqEventBus> logger,
        IOptions<RabbitmqOptions> options)
    {
        Guard.NotNull(config.Value, nameof(config));

        _logger = logger;
        _publisherConfig = config.Value;
        _connection = connection.GetAmqpConnection();
        _amqpChannels.Add((_connection.CreateModel(), new RabbitMqExchange()));
        _serializer = options.Value.Serializer ?? new DefaultNewtonSoftJsonSerializer();
    }

    private IModel InitChannel(RabbitMqExchange exchangeInfo, Dictionary<string, object> headers = default)
    {
        if (!string.IsNullOrWhiteSpace(exchangeInfo.ExchangeName) &&
            _amqpChannels.Any(a => a.exchange.ExchangeName == exchangeInfo.ExchangeName))
        {
            var channelWithExchange = _amqpChannels.First(a => a.exchange.ExchangeName == exchangeInfo.ExchangeName);

            if (channelWithExchange.channel.IsOpen)
            {
                return channelWithExchange.channel;
            }
            else
            {
                _amqpChannels.Remove(channelWithExchange);
            }
        }

        var newChannel = _connection.CreateModel();

        if (!string.IsNullOrWhiteSpace(exchangeInfo.ExchangeName))
        {
            var exchangeTypeValue = exchangeInfo.ExchangeType == default
                ? nameof(ExchangeType.Direct).ToLower()
                : nameof(exchangeInfo.ExchangeType).ToLower();

            newChannel.ExchangeDeclare(
                exchange: exchangeInfo.ExchangeName,
                type: exchangeTypeValue,
                durable: true,
                autoDelete: false,
                arguments: headers);
        }

        _amqpChannels.Add((newChannel, exchangeInfo));

        return newChannel;
    }

    public Task Execute(string queue, Event @event, Dictionary<string, string> headers = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(@event, nameof(@event));

        var queueInfo = @event.GetQueueInfo();
        var exchangeInfo = @event.GetExchangeInfo();

        var amqpChannel = InitChannel(
            exchangeInfo, MapToRabbitmqHeaders(headers));

        var properties = amqpChannel.CreateBasicProperties();

        amqpChannel.BasicPublish(
            exchangeInfo.ExchangeName,
            routingKey: queueInfo.RoutingKey,
            body: SerializeToRabbitMqMessage(@event),
            basicProperties: properties);

        return Task.CompletedTask;
    }

    public Task Execute<TEvent>(TEvent @event, Dictionary<string, string> headers,
        CancellationToken cancellationToken = default) where TEvent : Event
    {
        Guard.NotNull(@event, nameof(@event));

        var queueInfo = @event.GetQueueInfo();
        var exchangeInfo = @event.GetExchangeInfo();

        var amqpChannel = InitChannel(
            exchangeInfo,
            MapToRabbitmqHeaders(headers));

        var properties = amqpChannel.CreateBasicProperties();

        amqpChannel.BasicPublish(
            exchangeInfo.ExchangeName,
            routingKey: queueInfo.RoutingKey,
            body: SerializeToRabbitMqMessage(@event),
            basicProperties: properties);

        return Task.CompletedTask;
    }


    private byte[] SerializeToRabbitMqMessage<TObject>(TObject obj)
    {
        if (obj is null)
            return default;

        var json = _serializer.Serialize(obj);
        var finalBody = Encoding.UTF8.GetBytes(json);
        return finalBody;
    }

    private static Dictionary<string, object> MapToRabbitmqHeaders(Dictionary<string, string> headers)
    {
        return headers == null
            ? new Dictionary<string, object>()
            : headers.Select((a, _) => new KeyValuePair<string, object>(a.Key, a.Value)).ToDictionary();
    }
}