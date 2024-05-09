using Microsoft.Extensions.Hosting;

namespace Infra.Events.Rabbitmq;

public class RabbitMqStarterHostedService : BackgroundService
{
    private readonly RabbitMqService _rabbitMqService;

    public RabbitMqStarterHostedService(RabbitMqService rabbitMqService)
    {
        _rabbitMqService = rabbitMqService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _rabbitMqService.ExecuteAsync(stoppingToken);
    }
}