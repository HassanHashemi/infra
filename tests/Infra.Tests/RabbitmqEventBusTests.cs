using Autofac;
using Infra.Eevents;
using Infra.Tests.Event;
using Xunit;


namespace Infra.Tests;

public class RabbitmqEventBusTests
{
    [Fact]
    public async Task EventTest_WhenSendEvent_ShouldCallEventHandlerAsync()
    {
		//Arrange
		var provider = new ContainerBuilder()
            .AddLoggingInternal()
			.AddRabbitmqEventBusInternal()
            .AddTestResultStorageInternal()
			.Build();

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