﻿using Infra.Serialization.Json;

namespace Infra.Events.Kafka
{
    public class KafkaOptions
    {
        public IJsonSerializer Serializer { get; set; }
    }
}
