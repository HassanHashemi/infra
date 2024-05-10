using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infra.Events.Rabbitmq.Rabbitmq;

public class RabbitmqConnectionMultiplexer
{
    private readonly IOptions<RabbitmqOptions> _options;

    public RabbitmqConnectionMultiplexer(IOptions<RabbitmqOptions> options)
    {
        _options = options;
    }

    internal IConnection GetConnection()
    {
        ConnectionFactory connection = new ConnectionFactory
        {
            UserName = _options.Value.UserName,
            Password = _options.Value.Password,
            HostName = _options.Value.Host,
            VirtualHost = _options.Value.VirtualHost,
            DispatchConsumersAsync = true
        };
        var channel = connection.CreateConnection();
        return channel;
    }
}