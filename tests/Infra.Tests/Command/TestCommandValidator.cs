using Domain;
using Infra.Commands;

namespace Infra.Tests.Command;

public class TestCommandValidator : ICommandValidator<TestCommand>
{
	public ValueTask ValidateAsync(TestCommand command)
	{
		if(command.CommandParameter == null)
		{
			throw new DomainValidationException(nameof(command.CommandParameter));
		}

		return ValueTask.CompletedTask;
	}
}