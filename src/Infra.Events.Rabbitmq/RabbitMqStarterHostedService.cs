using Microsoft.Extensions.Hosting;

namespace Infra.Events.Rabbitmq;

public class RabbitMqStarterHostedService : BackgroundService
{
    private readonly RabbitMqConsumerService _rabbitMqConsumerService;

    public RabbitMqStarterHostedService(RabbitMqConsumerService rabbitMqConsumerService)
    {
        _rabbitMqConsumerService = rabbitMqConsumerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _rabbitMqConsumerService.ExecuteAsync(stoppingToken);
    }
}