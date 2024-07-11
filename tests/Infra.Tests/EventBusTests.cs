using Autofac;
using Infra.Eevents;
using Infra.Tests.Event;
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

    [Fact]
    public async Task EventTest_WhenSendEvent_ShouldCallEventHandlerAsync()
    {
        //Arrange
		var provider = new ContainerBuilder()
            .AddLoggingInternal()
			.AddSyncEventBusInternal()
            .AddTestResultStorageInternal()
			.Build();

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
		var provider = new ContainerBuilder()
			.AddLoggingInternal()
			.AddSyncEventBusInternal()
			.AddTestResultStorageInternal()
			.Build();

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