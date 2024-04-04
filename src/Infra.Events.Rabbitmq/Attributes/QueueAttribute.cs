namespace Infra.Events.Rabbitmq;

public class QueueAttribute : Attribute
{
    public string Name { get; set; }
    public string RoutingKey { get; set; }
}
