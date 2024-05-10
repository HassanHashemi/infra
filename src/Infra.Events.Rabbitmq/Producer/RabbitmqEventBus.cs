using Infra.Eevents;
using RabbitMQ.Client;
using System.Reflection;
using System.Text;
using Infra.Serialization.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Event = Domain.Event;

namespace Infra.Events.Rabbitmq;

public class RabbitmqEventBus : IEventBus
{
    private readonly IJsonSerializer _serializer;
    private readonly ILogger<RabbitmqEventBus> _logger;
    private readonly RabbitmqConnectionMultiplexer _connectionMultiplexer;

    public RabbitmqEventBus(
        ILogger<RabbitmqEventBus> logger,
        IOptions<RabbitmqOptions> rabbitmqOptions,
        RabbitmqConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _serializer = rabbitmqOptions.Value.Serializer ?? new DefaultNewtonSoftJsonSerializer();
    }

    private QueueAttribute GetQueueInfo<TEvent>(TEvent @event) where TEvent : Event
    {
        var topicInfo = @event.GetType()
            .GetCustomAttribute<QueueAttribute>();

        if (topicInfo != null)
            return topicInfo;

        return new QueueAttribute(@event.EventName);
    }

    public Task Execute<TEvent>(TEvent @event, Dictionary<string, string> headers, CancellationToken cancellationToken = default) where TEvent : Event
    {
        Guard.NotNull(@event, nameof(@event));

        var eventData = _serializer.Serialize(@event);
        var queueAttribute = GetQueueInfo(@event);

        using (var connection = _connectionMultiplexer.GetConnection())
        {
            using (var channel = connection.CreateModel())
            {
                byte[] body = Encoding.UTF8.GetBytes(eventData);

                channel.BasicPublish(
                    exchange: queueAttribute.ExchangeName ?? queueAttribute.QueueName,
                    routingKey: queueAttribute.RoutingKey ?? string.Empty,
                    basicProperties: null,
                    body: body);

                _logger.LogInformation("Published message to exchange: {Exchange} ,payload: {Payload}", queueAttribute.ExchangeName ?? "default", eventData);
            }
        }

        return Task.CompletedTask;
    }
}