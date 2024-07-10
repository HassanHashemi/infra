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

public class KafkaEventBusTests : EventBusTestsBase
{
    protected IContainer InitEventBus(ServiceCollection externalServices = null)
    {
        var config = InitConfiguration();

        var services = new ServiceCollection()
            .AddLogging(x => x.AddConsole());

        var builder = new ContainerBuilder();
        builder.Populate(services);

        if (externalServices != null)
            builder.Populate(externalServices);

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
    public async Task EventTest_WhenSendEvent_ShouldCallEventHandlerAsync()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddSingleton<EventResultStorage>();

        var provider = this.InitEventBus(services);
        var bus = provider.Resolve<IEventBus>();
        var storage = provider.Resolve<EventResultStorage>();

        //Act
        await bus.Execute(new TestEvent(), new Dictionary<string, string>());

        //Assert
        int reTries = 10;
        while (true)
        {
            if (storage.InternalEventResultHasBeenSet == 1)
            {
                Assert.True(storage.InternalEventResultHasBeenSet == 1);
                break;
            }

            if (reTries <= 0)
            {
                Assert.Fail("Message was not consumed");
            }

            reTries--;
            await Task.Delay(TimeSpan.FromSeconds(0.1));
        }

        await bus.Execute(new TestEvent { MustPropagate = true }, new Dictionary<string, string>());
        reTries = 10;
        while (true)
        {
            if (storage.IntegrationEventResultHasBeenSet == 1)
            {
                Assert.True(storage.IntegrationEventResultHasBeenSet == 1);
                break;
            }

            if (reTries <= 0)
            {
                Assert.Fail("Message was not consumed");
            }

            reTries--;
            await Task.Delay(TimeSpan.FromSeconds(0.1));
        }
    }
}