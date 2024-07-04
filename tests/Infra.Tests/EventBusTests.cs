using Autofac;
using Autofac.Extensions.DependencyInjection;
using Domain;
using Infra.Commands;
using Infra.Common.Decorators;
using Infra.Eevents;
using Infra.Events;
using Infra.Queries;
using Infra.Tests.Command;
using Infra.Tests.Event;
using Infra.Tests.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Infra.Tests
{
	public class EventBusTests
	{
		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
			.ConfigureContainer<ContainerBuilder>(builder =>
			{
			});

		private static IContainer InitContainer(ServiceCollection externalServices = null)
		{
			var internalServices = new ServiceCollection().AddLogging(x => x.AddConsole());
			var builder = new ContainerBuilder();
			builder.Populate(internalServices);

			if (externalServices != null)
				builder.Populate(externalServices);

			var scannedAssemblies = new[]
			{
				typeof(TestEvent).Assembly
			};

			builder.AddSyncEventBus();
			builder.AddSyncEventHandlers(scannedAssemblies);

			IContainer provider = builder.Build();
			return provider;
		}

		private static IConfiguration InitConfiguration()
		{
			return new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables()
				.Build();
		}

		[Fact]
		public async Task EventTest_WhenSendEvent_ShouldCallEventHandlerAsync()
		{
			var provider = InitContainer();
			var bus = provider.Resolve<IEventBus>();

			await bus.Execute(new TestEvent() { }, new Dictionary<string, string>());

			while (true)
			{
				if (EventResultStorage.ResultHasBeenSet)
				{
					Assert.True(EventResultStorage.ResultHasBeenSet);
					break;
				}
			}
		}

		[Fact]
		public async Task EventTest_WhenSendEvent_ShouldCallCustomFuncBeforeEventHandlerAsync()
		{
			var provider = InitContainer();
			var bus = provider.Resolve<IEventBus>();

			await bus.Execute(new TestEvent() { }, new Dictionary<string, string>());

			while (true)
			{
				if (EventResultStorage.ResultHasBeenSet)
				{
					Assert.True(EventResultStorage.ResultHasBeenSet);
					break;
				}
			}
		}
	}
}