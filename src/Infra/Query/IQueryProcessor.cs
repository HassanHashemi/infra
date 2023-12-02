using System.Threading;
using System.Threading.Tasks;

namespace Infra.Queries
{
    public interface IQueryProcessor
    {
        Task<TResult> ExecuteAsync<TResult>(IQueryResult<TResult> query, CancellationToken cts = default);
    }
}
