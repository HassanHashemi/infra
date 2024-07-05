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

public class RabbitmqEventBusTests : EventBusTests
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