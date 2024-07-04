using Infra.Commands;

namespace Infra.Tests.Command;

public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
	public Task<string> HandleAsync(TestCommand command, CancellationToken cancellationToken)
	{
		return Task.FromResult(1.ToString());
	}
}
