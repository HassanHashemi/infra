using System.Threading;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public abstract class MessageHandler : IMessageHandler
    {
        private readonly KafkaListenerCallbacks _callbacks;

        public MessageHandler(KafkaListenerCallbacks callbacks)
        {
            this._callbacks = callbacks;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._callbacks.MessageReceivedCallback += MessageReceived;
            return Task.CompletedTask;
        }

        protected virtual Task MessageReceived(BusMessageReceivedArgs e)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._callbacks.MessageReceivedCallback -= MessageReceived;
            return Task.CompletedTask;
        }
    }
}
