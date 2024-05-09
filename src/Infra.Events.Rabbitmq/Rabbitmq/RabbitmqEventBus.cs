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
    private readonly RabbitMqService _rabbitMqService;
    private readonly ILogger<RabbitmqEventBus> _logger;

    public RabbitmqEventBus(
        RabbitMqService rabbitMqService,
        ILogger<RabbitmqEventBus> logger,
        IOptions<RabbitmqOptions> rabbitmqOptions)
    {
        _logger = logger;
        _rabbitMqService = rabbitMqService;
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

        var connection = _rabbitMqService.GetConnection();

        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(
                queue: queueAttribute.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: headers.Select((x, _) => new KeyValuePair<string, object>(x.Key, x.Value)).ToDictionary());

            byte[] body = Encoding.UTF8.GetBytes(eventData);

            channel.BasicPublish(
                exchange: queueAttribute.ExchangeName ?? string.Empty,
                routingKey: queueAttribute.RoutingKey ?? queueAttribute.QueueName,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Published message to exchange: {Exchange} ,payload: {Payload}", queueAttribute.ExchangeName ?? "default", eventData);
        }

        return Task.CompletedTask;
    }
}