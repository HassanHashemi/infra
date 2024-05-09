using Infra.Eevents;
using RabbitMQ.Client;
using System.Reflection;
using System.Text;
using Autofac;
using Infra.Serialization.Json;
using Event = Domain.Event;

namespace Infra.Events.Rabbitmq;

public class RabbitmqEventBus : IEventBus
{
    private readonly ILifetimeScope _scope;
    private readonly IJsonSerializer _serializer;

    public RabbitmqEventBus(ILifetimeScope scope1)
    {
        _scope = scope1;
        _serializer = new DefaultNewtonSoftJsonSerializer();
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

        using (var scope = _scope.BeginLifetimeScope())
        {
            var rabbitMqService = scope.Resolve<RabbitMqService>();

            var connection = rabbitMqService.GetConnection();

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
                    routingKey: queueAttribute.RoutingKey?? queueAttribute.QueueName,
                    basicProperties: null,
                    body: body);
            }
        }

        return Task.CompletedTask;
    }
}