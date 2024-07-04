using Infra.Commands;

namespace Infra.Tests.Command;

public class TestCommand : ICommand
{
	public string CommandParameter { get; set; }
}
