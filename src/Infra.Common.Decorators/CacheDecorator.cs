using Infra.Queries;
using Infra.Serialization.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public class CacheDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQueryResult<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> _innerHandler;
        private readonly IDistributedCache _cache;
        private readonly IJsonSerializer _serializer;

        public CacheDecorator(
            IDistributedCache cache, 
            IQueryHandler<TQuery, TResult> inner,
            IOptions<QueryProcessorOptions> options)
        {
            _innerHandler = inner;
            _cache = cache;

            Guard.NotNull(options.Value, "queryProcessorOptions");

            if (options.Value.JsonSerializer is null)
                throw new ArgumentNullException(nameof(options.Value.JsonSerializer));

            _serializer = options.Value.JsonSerializer;
        }

        public Task<TResult> HandleAsync(TQuery parameters, CancellationToken cts)
        {
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
                        _serializer,
                        cts);
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
                        _serializer,
                        cts);
                }
            }
            else
            {
                return _innerHandler.HandleAsync(parameters, cts);
            }
        }
    }
}
