namespace Infra.Events.Rabbitmq;

public sealed class RabbitMqExchange
{
    private RabbitMqExchange()
    {
    }

    public RabbitMqExchange(string exchangeName = default, ExchangeType exchangeType = default, string routingKey = null)
    {
        ExchangeType = exchangeType;
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
    }

    public string ExchangeName { get; set; }
    public ExchangeType ExchangeType { get; set; }
    public string RoutingKey { get; set; }

    public bool Durable => true;
}