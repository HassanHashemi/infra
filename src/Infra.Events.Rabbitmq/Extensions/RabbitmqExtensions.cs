using RabbitMQ.Client;

namespace Infra.Events.Rabbitmq;

internal static class RabbitmqExtensions
{
    public static IConnection GetAmqpConnection(this IEnumerable<IConnection> amqpConnections)
        => amqpConnections.First(c =>
            c.ClientProperties.Contains(new("Connection", 0)));

}