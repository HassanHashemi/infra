using Infra.Serialization.Json;

namespace Infra.Commands;

public class CommandProcessorOptions
{
    public string EndServiceKey { get; set; } = "4";
    public IJsonSerializer JsonSerializer { get; set; }
}