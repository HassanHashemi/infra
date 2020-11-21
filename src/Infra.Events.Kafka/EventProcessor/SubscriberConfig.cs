using Confluent.Kafka;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Infra.Events.Kafka
{
    public class SubscriberConfig
    {
        public string[] Topics { get; set; }
        public string BootstrappServers { get; set; }
        public string GroupId { get; set; }
        public AutoOffsetReset OffsetResetType { get; set; } = AutoOffsetReset.Earliest;

        public bool IsValid
        {
            get
            {
                return 
                    this.Topics != null
                    &&
                    this.Topics.Any()
                    &&
                    !string.IsNullOrEmpty(this.BootstrappServers)
                    &&
                    !string.IsNullOrEmpty(this.GroupId);
            }
        }
    }
}
