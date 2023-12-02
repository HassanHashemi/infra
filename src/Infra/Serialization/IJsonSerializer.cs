using System;

namespace Infra.Serialization.Json
{
    public interface IJsonSerializer
    {
        public string Serialize(object input);
        public T Deserialize<T>(string json);
        public object Deserialize(string json, Type type);
    }
}
