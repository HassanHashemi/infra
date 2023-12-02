using System.Threading;
using System.Threading.Tasks;

namespace Infra.Commands
{
    public interface ICommandProcessor
    {
        Task<TResult> ExecuteAsync<TCommand, TResult>(TCommand command, CancellationToken cts = default);
    }
}