using Infra.Events;

namespace Infra.Tests.Event
{
	public class TestEventHandler : IEventHandler<TestEvent>
	{
		public Task HandleEvent(TestEvent @event)
		{
			EventResultStorage.ResultHasBeenSet = true;
			return Task.CompletedTask;
		}
	}
}
