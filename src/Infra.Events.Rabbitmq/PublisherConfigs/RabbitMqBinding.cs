namespace Infra.Events.Rabbitmq;

public sealed record RabbitMqBinding
{
    public string RoutingKey { get; init; }
    public Dictionary<string, object> BindingArguments { get; init; }
}