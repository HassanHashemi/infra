using Autofac;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Infra.Queries
{
    public sealed class QueryProcessor : IQueryProcessor
    {
        private readonly ILifetimeScope _container;

        public QueryProcessor(ILifetimeScope container)
        {
            this._container = container;
        }

        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(IQueryResult<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _container.ResolveKeyed("3", handlerType);
            return handler.HandleAsync((dynamic)query);
        }
    }
}
