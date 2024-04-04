using Infra.Serialization.Json;

namespace Infra.Events.Rabbitmq;

public class RabbitmqOptions
{
    public IJsonSerializer Serializer { get; set; }
}