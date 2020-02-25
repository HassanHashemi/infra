using System.Threading.Tasks;

namespace Infra.Commands
{
    public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand
    {
        Task<CommandResult<TResult>> HandleAsync(TCommand command);
    }
}