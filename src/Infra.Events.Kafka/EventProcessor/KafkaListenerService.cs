using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class KafkaListenerService : IHostedService
    {
        private readonly KafkaListener _kafkaListener;

        public KafkaListenerService(KafkaListener kafkaListener)
        {
            this._kafkaListener = kafkaListener;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return this._kafkaListener.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _kafkaListener.StopAsync(cancellationToken);
        }
    }
}