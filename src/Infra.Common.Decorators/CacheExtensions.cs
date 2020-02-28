using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

namespace Infra.Common.Decorators
{
    public static class CacheExtensions
    {
        public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache source,
            string key,
            Func<DistributedCacheEntryOptions, Task<T>> factory,
            CancellationToken cancellationToken)
        {
            var cachedResult = await source.GetStringAsync(key, cancellationToken);
            if (cachedResult != null)
            {
                return JsonConvert.DeserializeObject<T>(cachedResult);
            }
            else
            {
                var options = new DistributedCacheEntryOptions();

                // 1. invoke factory method to create new object
                var result = await factory(options);

                if (result == null)
                {
                    return default;
                }

                // 2. store the newly created object into cache
                await source.CreateEntry(key, result, options, cancellationToken);

                return result;
            }
        }

        private static Task CreateEntry(this IDistributedCache cache, string key, object value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
        {
            var jsonEntry = JsonConvert.SerializeObject(value);

            return cache.SetStringAsync(key, jsonEntry, options, cancellationToken);
        }
    }
}
