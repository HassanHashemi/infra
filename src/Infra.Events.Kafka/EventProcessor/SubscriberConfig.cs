using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Infra.Events.Kafka
{
    public class SubscriberConfig
    {
        public List<string> Topics { get; internal set; } = new List<string>();
        public string BootstrappServers { get; set; }
        public string GroupId { get; set; }
        public AutoOffsetReset OffsetResetType { get; set; } = AutoOffsetReset.Earliest;
        public bool EnableAutoCommit { get; set; } = false;
        public int? SessionTimeoutMs { get; set; }
        public int? MaxPollIntervalMs { get; set; }
        public Assembly[] EventAssemblies { get; set; }
        public Func<IServiceProvider, ValueTask<CultureInfo>> PreMessageHandlingHandler { get; set; } = null;

        public bool IsValid
        {
            get
            {
                return
                    this.EventAssemblies.Count() > 0
                    &&
                    !string.IsNullOrEmpty(this.BootstrappServers)
                    &&
                    !string.IsNullOrEmpty(this.GroupId);
            }
        }
    }
}
