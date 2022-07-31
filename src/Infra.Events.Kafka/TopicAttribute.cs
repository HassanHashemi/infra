using System;

namespace Infra.Events.Kafka
{
    public class TopicAttribute : Attribute
    {
        public string Name { get; set; }
        public int PartitionSize { get; set; } = 1;
    }
}
