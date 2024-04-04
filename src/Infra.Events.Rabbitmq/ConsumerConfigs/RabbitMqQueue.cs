namespace Infra.Events.Rabbitmq;

public sealed record RabbitMqQueue
{
    public string QueueName { get; init; }
    public bool Durable { get; init; }
    public bool AutoDelete => false;
}