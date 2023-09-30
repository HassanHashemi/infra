using Autofac;
using Infra.Commands;
using Infra.Queries;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
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

        public async Task<TResult> HandleAsync(TQuery parameters)
        {
            if (_config != null)
                await _config.Handler(parameters, _scope);

            return await _innerHandler.HandleAsync(parameters);
        }

    }

    public class CommandFuncDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand, TResult> _inner;
        private readonly ILogger<CommandLoggerDecorator<TCommand, TResult>> _logger;
        private readonly FuncDecoratorOptions _options;
        private readonly ILifetimeScope _scope;

        public CommandFuncDecorator(
            ICommandHandler<TCommand, TResult> inner,
            ILogger<CommandLoggerDecorator<TCommand, TResult>> logger,
            IOptions<FuncDecoratorOptions> options,
            ILifetimeScope scope)
        {
            _inner = inner;
            _logger = logger;
            _options = options.Value;
            _scope = scope;
        }

        public async Task<TResult> HandleAsync(TCommand command)
        {
            if (_options.Handler != null)
                await _options.Handler(command, _scope);

            return await _inner.HandleAsync(command);
        }
    }

    public class CacheDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQueryResult<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> _innerHandler;
        private readonly IDistributedCache _cache;

        public CacheDecorator(IDistributedCache cache, IQueryHandler<TQuery, TResult> inner)
        {
            _innerHandler = inner;
            _cache = cache;
        }

        public Task<TResult> HandleAsync(TQuery parameters)
        {
            using var cts = new CancellationTokenSource(5000);
            cts.CancelAfter(5000);

            if (parameters is CacheableQuery<TQuery, TResult> cache)
            {
                if (!cache.ReValidate)
                {
                    return _cache.GetOrCreateAsync(
                        cache.GetKey(),
                        (options) =>
                        {
                            options.AbsoluteExpiration = cache.AbsoluteExpiration;
                            // options.SlidingExpiration = cache.SlidingExpiration;
                            return _innerHandler.HandleAsync(parameters);
                        },
                        cts.Token);
                }
                else
                {
                    return _cache.CreateAsync(
                        cache.GetKey(),
                        (options) =>
                        {
                            options.AbsoluteExpiration = cache.AbsoluteExpiration;
                            return _innerHandler.HandleAsync(parameters);
                        },
                        cts.Token);
                }
            }
            else
            {
                return _innerHandler.HandleAsync(parameters);
            }
        }
    }
}
