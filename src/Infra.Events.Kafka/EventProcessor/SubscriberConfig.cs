using Confluent.Kafka;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infra.Events.Kafka
{
    public class SubscriberConfig
    {
        public List<string> Topics { get; internal set; } = new List<string>();
        public string BootstrappServers { get; set; }
        public string GroupId { get; set; }
        public AutoOffsetReset OffsetResetType { get; set; } = AutoOffsetReset.Earliest;
        public Assembly[] EventAssemblies { get; set; }

        public bool IsValid
        {
            get
            {
                return
                    this.EventAssemblies.Count() > 0
                    &&
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
