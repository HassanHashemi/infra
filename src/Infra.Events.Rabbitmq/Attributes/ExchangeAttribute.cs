namespace Infra.Events.Rabbitmq;

public class ExchangeAttribute : Attribute
{
    public string Name { get; set; }
    public ExchangeType ExchangeType { get; set; } = ExchangeType.Direct;
}