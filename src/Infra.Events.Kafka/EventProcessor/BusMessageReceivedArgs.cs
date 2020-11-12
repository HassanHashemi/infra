using System;

namespace Infra.Events.Kafka
{
    public class BusMessageReceivedArgs : EventArgs
    {
        public BusMessageReceivedArgs(string eventName, string data)
        {
            // TODO: Guard.NotNullOrEmpty
            Guard.NotNullOrEmpty(eventName, nameof(eventName));
            Guard.NotNullOrEmpty(data, nameof(data));

            this.EventName = eventName;
            this.Data = data;
        }

        public string EventName { get; }
        public string Data { get; }
    }
}
