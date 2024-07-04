using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infra.Events.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Infra.Events.Rabbitmq;
using Confluent.Kafka;
using Infra.Tests.Event;

namespace Flight.Gateway.Api;

public class Program
{
	public static IHostBuilder CreateHostBuilder(string[] args, bool useKafka = false, bool useRabbitmq = false) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureContainer<ContainerBuilder>(builder =>
				{
					var config = InitConfiguration();
					if (useKafka)
					{
						builder.AddKafka(p =>
						{
							p.BootstrapServers = config.GetConnectionString("Kafka");
						},
						consumer =>
						{
							consumer.OffsetResetType = AutoOffsetReset.Earliest;
							consumer.GroupId = "xunit-consumer-group";
							consumer.BootstrappServers = config.GetConnectionString("Kafka");
							consumer.EventAssemblies = new[] { typeof(TestEvent).Assembly };
							consumer.MaxPollIntervalMs = 50_000;
							consumer.SessionTimeoutMs = 50_000;
							consumer.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
						});
					}
					if (useRabbitmq)
					{
						builder.AddRabbitmqInternal(
							p =>
							{
								p.Host = config.GetConnectionString("Rabbitmq");
							},
							c =>
							{
								c.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
								c.EventAssemblies = new[] { typeof(TestEvent).Assembly };
								c.ConsumerGroupId = "xunit-consumer-group";
							});
					}
				});
	
	private static IConfiguration InitConfiguration()
	{
		return new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.AddEnvironmentVariables()
			.Build();
	}
}