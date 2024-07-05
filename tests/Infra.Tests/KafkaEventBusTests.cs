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

namespace Infra.Tests;

public class KafkaEventBusTests : EventBusTests
{
    protected override IContainer InitEventBus(ServiceCollection externalServices = null)
    {
        var config = InitConfiguration();

        var services = new ServiceCollection()
            .AddLogging(x => x.AddConsole());

        var builder = new ContainerBuilder();
        builder.Populate(services);

        var scannedAssemblies = new[]
        {
            typeof(TestEvent).Assembly
        };

        builder.AddKafka(producer =>
            {
                producer.BootstrapServers = config.GetConnectionString("Kafka");
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

        builder.AddSyncEventHandlers(scannedAssemblies);

        IContainer provider = builder.Build();
        return provider;
    }

    [Fact]
    public override async Task EventTest_WhenSendEvent_ShouldCallEventHandlerAsync()
    {
        var provider = this.InitEventBus();
        var bus = provider.Resolve<IEventBus>();

        await bus.Execute(new TestEvent(), new Dictionary<string, string>());
        while (true)
        {
            if (EventResultStorage.InternalEventResultHasBeenSet == 1)
            {
                Assert.True(EventResultStorage.InternalEventResultHasBeenSet == 1);
                break;
            }
        }

        await bus.Execute(new TestEvent { MustPropagate = true }, new Dictionary<string, string>());
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