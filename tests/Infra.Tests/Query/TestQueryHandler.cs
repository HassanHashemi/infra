using Infra.Queries;

namespace Infra.Tests.Query;

public class TestQueryHandler :
	IQueryHandler<TestQuery, string>,
	IQueryHandler<TestQueryCacheable, int>
{
	public Task<string> HandleAsync(TestQuery parameters, CancellationToken cts = default)
	{
		return Task.FromResult(1.ToString());
	}

	public Task<int> HandleAsync(TestQueryCacheable parameters, CancellationToken cts = default)
	{
		return Task.FromResult((parameters.HandlerCallCount + 1));
	}
}
