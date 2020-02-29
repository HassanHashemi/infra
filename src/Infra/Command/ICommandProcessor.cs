using System.Threading.Tasks;

namespace Infra.Commands
{
    public interface ICommandProcessor
    {
        Task<TResult> ExecuteAsync<TCommand, TResult>(TCommand command);
    }
}