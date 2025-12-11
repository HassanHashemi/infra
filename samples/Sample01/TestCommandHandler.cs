using System.Threading;
using System.Threading.Tasks;
using Infra.Commands;

public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<string> HandleAsync(TestCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(1.ToString());
    }
}