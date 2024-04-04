namespace Infra.Events.Rabbitmq;

public class ExchangeAttribute : Attribute
{
    public string Name { get; set; }
    public string RoutingKey { get; set; }
    public ExchangeType ExchangeType { get; set; } = ExchangeType.Direct;
}