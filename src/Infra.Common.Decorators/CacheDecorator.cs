using Infra.Queries;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
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
                return _cache.GetOrCreateAsync(
                        cache.GetKey(),
                            (options) =>
                            {
                                options.AbsoluteExpiration = cache.AbsoluteExpiration;
                                options.SlidingExpiration = cache.SlidingExpiration;

                                return _innerHandler.HandleAsync(parameters);
                            },
                            cts.Token);
            }
            else
            {
                return _innerHandler.HandleAsync(parameters);
            }
        }
    }
}
