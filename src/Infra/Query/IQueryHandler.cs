using System.Threading.Tasks;

namespace Infra.Queries
{
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQueryResult<TResult>
    {
        Task<TResult> HandleAsync(TQuery parameters);
    }
}