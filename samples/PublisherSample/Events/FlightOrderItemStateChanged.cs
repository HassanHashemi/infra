using Infra.Events.Kafka;
using Infra.Events.Rabbitmq;
using News.Domain;

namespace Sample;

[Topic(Name = "Ota.FlightOrderItem1")]
[Queue(QueueName = "FlightOrderItemStateChanged", ExchangeName = "FlightOrderItemStateChanged")]
public class FlightOrderItemStateChanged : DomainEvent
{
    public string Value { get; set; }
}