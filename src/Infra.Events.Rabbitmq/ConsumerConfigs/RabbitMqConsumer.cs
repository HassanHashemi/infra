using RabbitMQ.Client.Events;

namespace Infra.Events.Rabbitmq;

public sealed record RabbitMqConsumer<TConsumer> where TConsumer : AsyncEventingBasicConsumer
{
    public ushort PrefetchCount { get; init; }
    public bool GlobalPrefetchCount { get; init; }
    //public RabbitMqExchange ExchangeDetails { get; init; }
    public RabbitMqExchange DeadLetterExchangeDetails { get; init; }
    //public RabbitMqQueue QueueDetails { get; init; }
    //public RabbitMqBinding BindingDetails { get; init; }
    public bool AutoAcknowledgement { get; init; }
}