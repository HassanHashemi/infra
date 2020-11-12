using Confluent.Kafka;
using System;

namespace Infra.Events.Kafka
{
    public class SubscriberConfig
    {
        public string[] Topics { get; set; }
        public string BootstrappServers { get; set; }
#if DEBUG
            = "localhost:9092";
#endif
        public string GroupId { get; set; }
        public AutoOffsetReset OffsetResetType { get; set; } = AutoOffsetReset.Earliest;

        public bool IsValid
        {
            get
            {
                return this.Topics.ContainsElement()
                    &&
                    !string.IsNullOrEmpty(this.BootstrappServers)
                    &&
                    !string.IsNullOrEmpty(this.GroupId);
            }
        }
    }
}
