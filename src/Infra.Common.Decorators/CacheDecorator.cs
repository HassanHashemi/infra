using Infra.Queries;
using Infra.Serialization.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public class CacheDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQueryResult<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> _innerHandler;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly IJsonSerializer _serializer;

        public CacheDecorator(
            IDistributedCache cache,
            IMemoryCache memoryCache,
            IQueryHandler<TQuery, TResult> inner,
            IOptions<QueryProcessorOptions> options)
        {
            Guard.NotNull(options.Value, "queryProcessorOptions");

            if (options.Value.JsonSerializer is null)
                throw new ArgumentNullException(nameof(options.Value.JsonSerializer));

            _innerHandler = inner;
            _distributedCache = cache;
            _memoryCache = memoryCache;
            _serializer = options.Value.JsonSerializer;
        }

        public Task<TResult> HandleAsync(TQuery parameters, CancellationToken cts)
        {
            if (!(parameters is CacheableQuery<TQuery, TResult> cache))
            {
                return _innerHandler.HandleAsync(parameters, cts);
            }
            
            if (!cache.ReValidate)
            {
                return cache.Storage switch
                {
                    CacheStorage.Redis => _distributedCache.GetOrCreateAsync(
                        cache.GetKey(),
                        options =>
                        {
                            options.AbsoluteExpiration = cache.AbsoluteExpiration;
                            return _innerHandler.HandleAsync(parameters, cts);
                        },
                        _serializer,
                        cts),
                    CacheStorage.Memory => _memoryCache.GetOrCreateAsync(
                        cache.GetKey(),
                        options => 
                        {
                            options.AbsoluteExpiration = cache.AbsoluteExpiration;
                            return _innerHandler.HandleAsync(parameters, cts);
                        }),
                    _ => throw new NotSupportedException("Only memory and Redis are supported")
                };
            }
            else
            {
                return cache.Storage switch
                {
                    CacheStorage.Redis => _distributedCache.CreateAsync(
                        cache.GetKey(),
                        options =>
                        {
                            options.AbsoluteExpiration = cache.AbsoluteExpiration;

                            return _innerHandler.HandleAsync(parameters);
                        },
                        _serializer,
                        cts),
                    CacheStorage.Memory => CreateInMemoryEntry(cache, parameters, cts),
                    _ => throw new ArgumentException("Only Redis and memory cache are supported")
                };
            }
        }

        private async Task<TResult> CreateInMemoryEntry(CacheableQuery<TQuery, TResult> cache, TQuery query, CancellationToken cts)
        {
            var result = await _innerHandler.HandleAsync(query, cts);

            _memoryCache.Set(cache.GetKey(), result, new MemoryCacheEntryOptions 
            {
                SlidingExpiration = cache.SlidingExpiration,
                AbsoluteExpiration = cache.AbsoluteExpiration,
            });

            return result;
        }
    }
}
