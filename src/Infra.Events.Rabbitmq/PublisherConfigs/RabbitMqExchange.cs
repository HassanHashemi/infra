namespace Infra.Events.Rabbitmq;

public sealed record RabbitMqExchange
{
    public string ExchangeName { get; init; }
    public ExchangeType ExchangeType { get; init; }
    public bool Durable => true;
}