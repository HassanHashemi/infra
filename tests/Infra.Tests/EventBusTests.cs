using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infra.Eevents;
using Infra.Events;
using Infra.Tests.Event;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Infra.Tests;

public class EventBusTests : EventBusTestsBase
{
    private readonly ITestOutputHelper _console;

    public EventBusTests(ITestOutputHelper console)
    {
        _console = console;
    }

    public IContainer InitEventBus(ServiceCollection externalServices = null)
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
    public async Task EventTest_WhenSendEvent_ShouldCallEventHandlerAsync()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddSingleton<EventResultStorage>();

        var provider = InitEventBus(services);
        var bus = provider.Resolve<IEventBus>();

        //Act
        await bus.Execute(new TestEvent(), new Dictionary<string, string>());

        //Assert
        var storage = provider.Resolve<EventResultStorage>();
        while (true)
        {
            if (storage.InternalEventResultHasBeenSet == 1)
            {
                Assert.True(storage.InternalEventResultHasBeenSet == 1);
                break;
            }
        }
    }

    [Fact]
    public async Task EventTest_WhenSendEvent_ShouldCallCustomFuncBeforeEventHandlerAsync()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddSingleton<EventResultStorage>();

        var provider = InitEventBus(services);
        var bus = provider.Resolve<IEventBus>();

        //Act
        await bus.Execute(new TestEvent(), new Dictionary<string, string>());

        //Assert
        var storage = provider.Resolve<EventResultStorage>();
        while (true)
        {
            if (storage.InternalEventResultHasBeenSet > 1)
            {
                Assert.Fail("Consumer called more than once");
                break;
            }

            if (storage.InternalEventResultHasBeenSet == 1)
            {
                Assert.True(storage.InternalEventResultHasBeenSet == 1);
                break;
            }
        }
    }
}