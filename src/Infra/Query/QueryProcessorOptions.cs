using Infra.Serialization.Json;
using System;

namespace Infra.Queries
{
    public class QueryProcessorOptions
    {
        public string EndServiceKey { get; set; } = "4";
        public IJsonSerializer JsonSerializer { get; set; }
    }
}
