using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infra.Eevents;
using Infra.Events;
using Infra.Events.Rabbitmq;
using Infra.Tests.Event;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Infra.Tests;

public class RabbitmqEventBusTests : EventBusTestsBase
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