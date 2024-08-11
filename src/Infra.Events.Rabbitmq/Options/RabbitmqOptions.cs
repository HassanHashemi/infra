using Infra.Serialization.Json;

namespace Infra.Events.Rabbitmq;

public class RabbitmqOptions
{
    public string Host { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public IJsonSerializer Serializer { get; set; }
}