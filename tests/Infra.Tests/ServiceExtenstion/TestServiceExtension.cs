using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infra.Common.Decorators;
using Infra.Eevents;
using Infra.EFCore;
using Infra.Events;
using Infra.Tests.Domain;
using Infra.Tests.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Infra.Events.Kafka;
using Infra.Events.Rabbitmq;
using Infra.Tests.Command;

namespace Infra.Tests;

internal static class TestServiceExtension
{
	private static IConfiguration Configuration => new ConfigurationBuilder()
		.AddJsonFile("appsettings.json")
		.AddEnvironmentVariables()
		.Build();

	internal static ContainerBuilder AddLoggingInternal(this ContainerBuilder builder)
	{
		var services = new ServiceCollection().AddLogging(x => x.AddConsole());
		builder.Populate(services);
		return builder;
	}

	internal static ContainerBuilder AddMemoryCacheInternal(this ContainerBuilder builder)
	{
		var services = new ServiceCollection();
		services
			.AddMemoryCache()
			.AddDistributedMemoryCache();

		builder.Populate(services);
		return builder;
	}

	internal static ContainerBuilder AddRedisCacheInternal(this ContainerBuilder builder)
	{
		var services = new ServiceCollection();
		services
			.AddMemoryCache()
			.AddStackExchangeRedisCache(o =>
			{
				o.Configuration = Configuration.GetConnectionString("Redis");
			});

		builder.Populate(services);
		return builder;
	}

	internal static ContainerBuilder AddDbContextInternal(this ContainerBuilder builder)
	{
		var services = new ServiceCollection();

		services.AddDbContext<TestDbContext>(options =>
			options.UseInMemoryDatabase(databaseName: "TestDb"));

		services.AddScoped<DbContext, TestDbContext>();

		builder.Populate(services);

		return builder;
	}

	internal static ContainerBuilder AddTestResultStorageInternal(this ContainerBuilder builder)
	{
		var services = new ServiceCollection();
		services.AddSingleton<EventResultStorage>();

		builder.Populate(services);
		return builder;
	}

	internal static ContainerBuilder AddUnitOfWorkInternal(this ContainerBuilder builder)
	{
		var scannedAssemblies = new[]
		{
			typeof(TestAggregateRootInfoUpdatedDomainEvent).Assembly
		};

		builder.Register<IUnitOfWork>(context =>
			{
				var db = context.Resolve<DbContext>();
				var logger = context.Resolve<ILogger<EfUnitOfWork>>();
				var syncEventBus = context.Resolve<SyncEventBus>();
				var eventBus = context.Resolve<IEventBus>();

				return new EfUnitOfWork(db, eventBus, syncEventBus, logger);
			})
			.InstancePerLifetimeScope();

		builder.AddCommandQuery(scannedAssemblies: scannedAssemblies);

		return builder;
	}

	internal static ContainerBuilder AddCommandQueryInternal(this ContainerBuilder builder)
	{
		var scannedAssemblies = new[]
		{
			typeof(TestCommand).Assembly
		};

		builder.AddCommandQuery(scannedAssemblies: scannedAssemblies);

		return builder;
	}

	internal static ContainerBuilder AddSyncEventBusInternal(this ContainerBuilder builder)
	{
		var scannedAssemblies = new[]
		{
			typeof(TestEvent).Assembly
		};

		builder.AddSyncEventBus();
		builder.AddSyncEventHandlers(scannedAssemblies);

		return builder;
	}

	internal static ContainerBuilder AddKafkaEventBusInternal(this ContainerBuilder builder)
	{
		var scannedAssemblies = new[]
		{
			typeof(TestEvent).Assembly
		};

		builder.AddKafka(
			producer =>
			{
				producer.BootstrapServers = Configuration.GetConnectionString("Kafka");
			},
			consumer =>
			{
				consumer.OffsetResetType = AutoOffsetReset.Earliest;
				consumer.GroupId = "xunit-consumer-group";
				consumer.BootstrappServers = Configuration.GetConnectionString("Kafka");
				consumer.EventAssemblies = scannedAssemblies;
				consumer.MaxPollIntervalMs = 50_000;
				consumer.SessionTimeoutMs = 50_000;
				consumer.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
			});

		builder.AddSyncEventHandlers(scannedAssemblies);
		return builder;
	}

	internal static ContainerBuilder AddRabbitmqEventBusInternal(this ContainerBuilder builder)
	{
		var scannedAssemblies = new[]
		{
			typeof(TestEvent).Assembly
		};

		builder.AddRabbitmqInternal(
			p =>
			{
				p.Host = "localhost";
				p.UserName = "rabbitmq";
				p.Password = "rabbitmq";
				p.VirtualHost = "/";
			},
			c =>
			{
				c.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
				c.EventAssemblies = new[] { typeof(TestEvent).Assembly };
			});

		builder.AddSyncEventHandlers(scannedAssemblies);

		return builder;
	}
}