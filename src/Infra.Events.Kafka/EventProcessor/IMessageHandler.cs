using Microsoft.Extensions.Hosting;

namespace Infra.Events.Kafka
{
    public interface IMessageHandler : IHostedService
    {
    }
}
