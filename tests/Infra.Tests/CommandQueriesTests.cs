using Autofac;
using Autofac.Extensions.DependencyInjection;
using Domain;
using Infra.Commands;
using Infra.Queries;
using Infra.Tests.Command;
using Infra.Tests.Query;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Infra.Tests;

public class CommandQueriesTests
{
	private static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.UseServiceProviderFactory(new AutofacServiceProviderFactory())
		.ConfigureContainer<ContainerBuilder>(builder =>
		{
		});

	[Fact]
	public async Task CommandTest_WhenSendCommand_ShouldCallCommandHandlerAsync()
	{
		//Arrange
		var provider = new ContainerBuilder()
			.AddLoggingInternal()
			.AddCommandQueryInternal()
			.Build();

		var processor = provider.Resolve<ICommandProcessor>();

		var result = await processor.ExecuteAsync<TestCommand, string>(new TestCommand()
		{
			CommandParameter = "value"
		});

		Assert.True(!string.IsNullOrWhiteSpace(result));
	}

	[Fact]
	public async Task CommandTest_WhenSendCommand_ShouldCallCommandValidatorThenHandlerAsync()
	{
		//Arrange
		var provider = new ContainerBuilder()
			.AddLoggingInternal()
			.AddCommandQueryInternal()
			.Build();

		var processor = provider.Resolve<ICommandProcessor>();

		var result = await processor.ExecuteAsync<TestCommand, string>(new TestCommand()
		{
			CommandParameter = "value"
		});
		Assert.True(!string.IsNullOrWhiteSpace(result));
		try
		{
			var resultWithException = await processor.ExecuteAsync<TestCommand, string>(new TestCommand()
			{
				CommandParameter = null
			});
		}
		catch (DomainValidationException)
		{
			Assert.True(true);
		}
	}

	[Fact]
	public async Task QueryTest_WhenSendQuery_ShouldCallQueryHandlerAsync()
	{
		//Arrange
		var provider = new ContainerBuilder()
			.AddLoggingInternal()
			.AddMemoryCacheInternal()
			.AddCommandQueryInternal()
			.Build();

		var processor = provider.Resolve<IQueryProcessor>();

		var result = await processor.ExecuteAsync<string>(new TestQuery(), new CancellationToken());

		Assert.True(!string.IsNullOrWhiteSpace(result));
	}

	[Fact]
	public async Task CacheableQueryTest_WhenQueryCached_ShouldNotCallQueryHandler()
	{
		//Arrange
		var provider = new ContainerBuilder()
			.AddLoggingInternal()
			.AddMemoryCacheInternal()
			.AddCommandQueryInternal()
			.Build();

		var processor = provider.Resolve<IQueryProcessor>();

		var result = await processor.ExecuteAsync(
			new TestQueryCacheable()
			{
			},
			new CancellationToken());
		Assert.True(result > 0);

		var resultCacheable = await processor.ExecuteAsync(
			new TestQueryCacheable()
			{
				HandlerCallCount = result
			},
			new CancellationToken());
		Assert.True(resultCacheable > 0);
		Assert.True(resultCacheable == result);
	}

	[Fact]
	public async Task CacheableQueryTest_WhenQueryCachedAndRevaldateRequested_ShouldCallQueryHandler()
	{
		//Arrange
		var provider = new ContainerBuilder()
			.AddLoggingInternal()
			.AddMemoryCacheInternal()
			.AddCommandQueryInternal()
			.Build();

		var processor = provider.Resolve<IQueryProcessor>();

		var result = await processor.ExecuteAsync(
			new TestQueryCacheable()
			{
			},
			new CancellationToken());
		Assert.True(result > 0);

		var resultRevalidated = await processor.ExecuteAsync(
			new TestQueryCacheable()
			{
				ReValidate = true,
				HandlerCallCount = result
			},
			new CancellationToken());
		Assert.True(resultRevalidated > 0);
		Assert.True(resultRevalidated > result);
	}

	[Fact]
	public async Task CacheableQueryTestWithRedisCache_WhenQueryCached_ShouldNotCallQueryHandler()
	{
		//Arrange
		var provider = new ContainerBuilder()
			.AddLoggingInternal()
			.AddRedisCacheInternal()
			.AddCommandQueryInternal()
			.Build();

		var processor = provider.Resolve<IQueryProcessor>();

		var result = await processor.ExecuteAsync(
			new TestQueryCacheable()
			{
			},
			new CancellationToken());
		Assert.True(result > 0);

		var resultCacheable = await processor.ExecuteAsync(
			new TestQueryCacheable()
			{
				HandlerCallCount = result
			},
			new CancellationToken());
		Assert.True(resultCacheable > 0);
		Assert.True(resultCacheable == result);
	}

	[Fact]
	public async Task CacheableQueryTestWithRedisCache_WhenQueryCachedAndRevaldateRequested_ShouldCallQueryHandler()
	{
		//Arrange
		var provider = new ContainerBuilder()
			.AddLoggingInternal()
			.AddRedisCacheInternal()
			.AddCommandQueryInternal()
			.Build();

		var processor = provider.Resolve<IQueryProcessor>();

		var result = await processor.ExecuteAsync(
			new TestQueryCacheable()
			{
			},
			new CancellationToken());
		Assert.True(result > 0);

		var resultRevalidated = await processor.ExecuteAsync(
			new TestQueryCacheable()
			{
				ReValidate = true,
				HandlerCallCount = result
			},
			new CancellationToken());
		Assert.True(resultRevalidated > 0);
		Assert.True(resultRevalidated > result);
	}
}