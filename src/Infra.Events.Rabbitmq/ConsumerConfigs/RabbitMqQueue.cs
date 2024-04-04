namespace Infra.Events.Rabbitmq;

public sealed class RabbitMqQueue
{
    private RabbitMqQueue()
    {
    }

    public RabbitMqQueue(string queueName, string routingKey = default)
    {
        Guard.NotNullOrEmpty(queueName, nameof(queueName));

        QueueName = queueName;
        RoutingKey = routingKey;
    }

    public string QueueName { get; set; }
    public string RoutingKey { get; set; }
    public bool Durable => true;
    public bool AutoDelete => false;
}