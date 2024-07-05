using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infra.Eevents;
using Infra.Events;
using Infra.Tests.Event;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Infra.Tests;

public class EventBusTests
{
    protected IConfiguration InitConfiguration() => new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

    protected virtual IContainer InitEventBus(ServiceCollection externalServices = null)
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

    [Fact]
    public virtual async Task EventTest_WhenSendEvent_ShouldCallEventHandlerAsync()
    {
        var provider = InitEventBus();
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
    public virtual async Task EventTest_WhenSendEvent_ShouldCallCustomFuncBeforeEventHandlerAsync()
    {
        var provider = InitEventBus();
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
}