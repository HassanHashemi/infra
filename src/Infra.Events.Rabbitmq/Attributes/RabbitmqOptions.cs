using Infra.Serialization.Json;

namespace Infra.Events.Rabbitmq;

public class RabbitmqOptions
{
    public string Host { get; set; } = "localhost";
    public string Username { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
    public IJsonSerializer Serializer { get; set; }
}