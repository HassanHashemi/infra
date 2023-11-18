using Newtonsoft.Json;
using System;

namespace Infra.Events.Kafka
{
    public class DefaultNewtonSoftJsonSerializer : IJsonSerializer
    {
        private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            Error = (e, args) => 
            {
                Console.WriteLine($"********* Serializer error {args.ErrorContext.Error} *********");
                args.ErrorContext.Handled = true;
            },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = PrivateSetterResolver.Instance
        };

        public T Deserialize<T>(string json) where T : class
        {
            Guard.NotNullOrEmpty(json, nameof(json));

            return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
        }

        public object Deserialize(string json, Type type)
        {
            Guard.NotNullOrEmpty(json, nameof(json));

            return JsonConvert.DeserializeObject(json, type, _jsonSerializerSettings);
        }

        public string Serialize(object input)
        {
            Guard.NotNull(input, nameof(input));
                
            return JsonConvert.SerializeObject(input, _jsonSerializerSettings);
        }
    }
}
