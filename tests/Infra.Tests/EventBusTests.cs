using Autofac;
using Autofac.Extensions.DependencyInjection;
using Confluent.Kafka;
using Infra.Eevents;
using Infra.Events;
using Infra.Events.Kafka;
using Infra.Tests.Event;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Infra.Tests
{
	public class EventBusTests
	{
		
		private static IContainer InitSyncEventBus(ServiceCollection externalServices = null)
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

		private static IContainer InitKafkaEventBus(ServiceCollection externalServices = null)
		{
			var config = InitConfiguration();
			var internalServices = new ServiceCollection().AddLogging(x => x.AddConsole());
			var builder = new ContainerBuilder();
			builder.Populate(internalServices);

			if (externalServices != null)
				builder.Populate(externalServices);

			var scannedAssemblies = new[]
			{
				typeof(TestEvent).Assembly
			};

			builder.AddKafka(p =>
			{
				p.BootstrapServers = config.GetConnectionString("Kafka");
			},
			consumer =>
			{
				consumer.OffsetResetType = AutoOffsetReset.Earliest;
				consumer.GroupId = "xunit-consumer-group";
				consumer.BootstrappServers = config.GetConnectionString("Kafka");
				consumer.EventAssemblies = scannedAssemblies;
				consumer.MaxPollIntervalMs = 50_000;
				consumer.SessionTimeoutMs = 50_000;
				consumer.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
			});

			//builder.AddSyncEventHandlers(scannedAssemblies);

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
			var provider = InitSyncEventBus();
			var bus = provider.Resolve<IEventBus>();

			await bus.Execute(new TestEvent() { }, new Dictionary<string, string>());

			while (true)
			{
				if (EventResultStorage.InternalEventResultHasBeenSet == 1)
				{
					Assert.True(EventResultStorage.InternalEventResultHasBeenSet == 1);
					break;
				}
			}
		}

		[Fact]
		public async Task EventTest_WhenSendEvent_ShouldCallCustomFuncBeforeEventHandlerAsync()
		{
			var provider = InitSyncEventBus();
			var bus = provider.Resolve<IEventBus>();

			await bus.Execute(new TestEvent() { }, new Dictionary<string, string>());

			while (true)
			{
				if (EventResultStorage.InternalEventResultHasBeenSet == 1)
				{
					Assert.True(EventResultStorage.InternalEventResultHasBeenSet == 1);
					break;
				}
			}
		}

		[Fact]
		public async Task KafkaIntegrationEventTest_WhenSendEvent_ShouldCallMessageHandlerAsync()
		{
			var provider = InitKafkaEventBus();
			var bus = provider.Resolve<IEventBus>();

			await bus.Execute(new TestEvent() { }, new Dictionary<string, string>());
			while (true)
			{
				if (EventResultStorage.InternalEventResultHasBeenSet == 1)
				{
					Assert.True(EventResultStorage.InternalEventResultHasBeenSet == 1);
					break;
				}
			}

			await bus.Execute(new TestEvent() { MustPropagate = true }, new Dictionary<string, string>());
			while (true)
			{
				if (EventResultStorage.IntegrationEventResultHasBeenSet == 1)
				{
					Assert.True(EventResultStorage.IntegrationEventResultHasBeenSet == 1);
					break;
				}
			}
		}
	}
}