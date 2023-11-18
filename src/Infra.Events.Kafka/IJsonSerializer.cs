using System;

namespace Infra.Events.Kafka
{
    public interface IJsonSerializer
    {
        public string Serialize(object input);
        public T Deserialize<T>(string json) where T : class;
        public object Deserialize(string json, Type type);
    }
}
