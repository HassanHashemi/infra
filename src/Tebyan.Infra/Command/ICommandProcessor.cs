using System.Threading.Tasks;

namespace Infra.Commands
{
    public interface ICommandProcessor
    {
        Task<CommandResult<TResult>> ExecuteAsync<TCommand, TResult>(TCommand command);
    }
}