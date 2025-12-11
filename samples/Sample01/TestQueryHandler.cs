using System.Threading;
using System.Threading.Tasks;
using Infra.Queries;

namespace Sample01;

public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public Task<string> HandleAsync(TestQuery parameters, CancellationToken cts = default)
    {
        return Task.FromResult(1.ToString());
    }
}