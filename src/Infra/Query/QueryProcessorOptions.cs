using System.Diagnostics.Contracts;

namespace Infra.Queries
{
    public class QueryProcessorOptions
    {
        public string EndServiceKey { get; set; } 
    }

    public class CommandProcessorOptions
    {
        public string EndServiceKey { get; set; }
    }
}
