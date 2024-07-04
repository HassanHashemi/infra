using Infra.Events;
using Infra.Events.Kafka;

namespace Infra.Tests.Event
{
	public class TestEventHandler :
		IEventHandler<TestEvent>,
		IMessageHandler<TestEvent>
	{
		public Task Handle(TestEvent @event, Dictionary<string, string> headers)
		{
			EventResultStorage.IntegrationEventResultHasBeenSet++;
			return Task.CompletedTask;
		}

		public Task HandleEvent(TestEvent @event)
		{
			EventResultStorage.InternalEventResultHasBeenSet++;
			return Task.CompletedTask;
		}
	}
}
