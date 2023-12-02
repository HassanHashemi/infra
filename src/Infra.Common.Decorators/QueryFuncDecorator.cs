using Autofac;
using Infra.Queries;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public class QueryFuncDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQueryResult<TResult>
    {
        private readonly ILifetimeScope _scope;
        private readonly FuncDecoratorOptions _config;
        private readonly IQueryHandler<TQuery, TResult> _innerHandler;

        public QueryFuncDecorator(ILifetimeScope scope, IOptions<FuncDecoratorOptions> config, IQueryHandler<TQuery, TResult> inner)
        {
            this._scope = scope;
            this._config = config.Value;
            this._innerHandler = inner;
        }

        public async Task<TResult> HandleAsync(TQuery parameters, CancellationToken cts)
        {
            if (_config != null)
                await _config.Handler(parameters, _scope);

            return await _innerHandler.HandleAsync(parameters, cts);
        }

    }
}
