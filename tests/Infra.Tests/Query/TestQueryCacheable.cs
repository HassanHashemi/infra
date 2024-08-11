using Infra.Queries;

namespace Infra.Tests.Query;

public class TestQueryCacheable : CacheableQuery<TestQueryCacheable, int>
{
	public int HandlerCallCount { get; internal set; }

	public override string GetKey() => nameof(TestQueryCacheable);
	public override DateTimeOffset? AbsoluteExpiration => DateTime.Now + TimeSpan.FromMinutes(5);
}
