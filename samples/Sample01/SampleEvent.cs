using Infra.Events.Kafka;
using Infra.Events.Rabbitmq;
using Domain;

namespace Sample01
{
    [Topic(Name = "Ota.FlightOrderItem1")]
    [Queue(QueueName = "FlightOrderItemStateChanged", ExchangeName = "FlightOrderItemStateChanged")]
    public class SampleEvent : DomainEvent
    {
        public string Value { get; set; }
    }
}