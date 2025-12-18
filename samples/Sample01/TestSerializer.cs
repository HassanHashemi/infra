using System;
using Infra.Serialization.Json;

namespace Sample01;

public class TestSerializer : IJsonSerializer
{
    public T Deserialize<T>(string json)
    {
        throw new NotImplementedException();
    }

    public object Deserialize(string json, Type type)
    {
        throw new NotImplementedException();
    }

    public string Serialize(object input)
    {
        throw new NotImplementedException();
    }
}