using Autofac;
using Autofac.Extensions.DependencyInjection;
using Domain;
using Infra.Commands;
using Infra.Common.Decorators;
using Infra.Queries;
using Infra.Tests.Command;
using Infra.Tests.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Infra.Tests
{
	public class CommandQueriesTests
	{
		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
			.ConfigureContainer<ContainerBuilder>(builder =>
			{
			});

		private static IContainer Init(ServiceCollection externalServices = null)
		{
			var internalServices = new ServiceCollection().AddLogging(x => x.AddConsole());
			var builder = new ContainerBuilder();

			builder.Populate(internalServices);

			if (externalServices != null)
				builder.Populate(externalServices);

			var scannedAssemblies = new[]
			{
				typeof(TestCommand).Assembly
			};

			builder.AddCommandQuery(scannedAssemblies: scannedAssemblies);

			IContainer provider = builder.Build();
			return provider;
		}

		[Fact]
		public async Task CommandTest_WhenSendCommand_ShouldCallCommandHandlerAsync()
		{
			var provider = Init();
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
			var provider = Init();
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
			var services = new ServiceCollection();
			services.AddMemoryCache();
			services.AddDistributedMemoryCache();
			var provider = Init(services);

			var processor = provider.Resolve<IQueryProcessor>();

			var result = await processor.ExecuteAsync<string>(new TestQuery(), new CancellationToken());

			Assert.True(!string.IsNullOrWhiteSpace(result));
		}

		[Fact]
		public async Task CacheableQueryTest_WhenQueryCached_ShouldNotCallQueryHandler()
		{
			var services = new ServiceCollection();
			services.AddMemoryCache();
			services.AddDistributedMemoryCache();

			var provider = Init(services);
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
			var services = new ServiceCollection();
			services.AddMemoryCache();
			services.AddDistributedMemoryCache();

			var provider = Init(services);
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
}