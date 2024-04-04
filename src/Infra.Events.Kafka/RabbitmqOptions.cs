using Infra.Serialization.Json;

namespace Infra.Events.Kafka
{
    public class RabbitmqOptions
    {
        public IJsonSerializer Serializer { get; set; }
    }
}