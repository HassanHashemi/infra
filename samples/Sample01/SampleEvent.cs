using Infra.Events.Kafka;
using Infra.Events.Rabbitmq;
using News.Domain;

namespace Orders.Domain.Events
{
    [Topic(Name = "Ota.FlightOrderItem1")]
    [Queue(QueueName = "FlightOrderItemStateChanged", ExchangeName = "FlightOrderItemStateChanged")]
    public class SampleEvent : DomainEvent
    {
        public string Value { get; set; }
    }
}