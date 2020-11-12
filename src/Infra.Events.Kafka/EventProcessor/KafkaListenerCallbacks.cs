using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public delegate Task MessageHandlerCallback(BusMessageReceivedArgs e);

    public class KafkaListenerCallbacks
    {
        internal KafkaListenerCallbacks()
        {
        }

        //internal EventHandler<BusMessageReceivedArgs> MessageReceived;
        internal MessageHandlerCallback MessageReceivedCallback = null;

        internal Task Invoke(object sender, BusMessageReceivedArgs e)
        {
            var handlers = this.MessageReceivedCallback;
            if (handlers != null)
            {
                return handlers(e);
            }

            return Task.CompletedTask;

            //var handlers = this.MessageReceived;

            //if (handlers != null)
            //{
            //    return handlers.Invoke(sender, e);
            //}
        }
    }
}
