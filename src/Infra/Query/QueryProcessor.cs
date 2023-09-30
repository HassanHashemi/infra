using Autofac;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Infra.Queries
{
    public sealed class QueryProcessor : IQueryProcessor
    {
        private readonly ILifetimeScope _container;
        private readonly QueryProcessorOptions _options;

        public QueryProcessor(ILifetimeScope container, IOptions<QueryProcessorOptions> options)
        {
            if (options.Value is null)
                throw new ArgumentException("QueryProcessorOptions must be set");

            _container = container;
            _options = options.Value;
        }

        [DebuggerStepThrough]
        public Task<TResult> ExecuteAsync<TResult>(IQueryResult<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _container.ResolveKeyed(_options.EndServiceKey, handlerType);
            return handler.HandleAsync((dynamic)query);
        }
    }
}
