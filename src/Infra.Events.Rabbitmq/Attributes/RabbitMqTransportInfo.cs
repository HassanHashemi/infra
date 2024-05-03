using MassTransit.Transports.Fabric;

namespace Infra.Events.Rabbitmq;

public sealed class RabbitMqTransportInfo
{
    private RabbitMqTransportInfo()
    {
    }

    public RabbitMqTransportInfo(string exchangeName, string queueName, ExchangeType exchangeType = default, string routingKey = null)
    {
        ExchangeType = exchangeType;
        ExchangeName = exchangeName;
        QueueName = queueName;
        RoutingKey = routingKey;
    }

    public string ExchangeName { get; set; }
    public string QueueName { get; set; }
    public ExchangeType ExchangeType { get; set; }
    public string RoutingKey { get; set; }
}