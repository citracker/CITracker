using Microsoft.Extensions.Caching.Memory;
using Shared.Interfaces;

namespace Shared.Implementations
{
    public class MemoryCacheManager : IMemoryCacheManager
    {
        private readonly IMemoryCache _memoryCache;
        public MemoryCacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public async Task SetCache<T>(string cacheKey, T value)
        {
            try
            {
                await Task.Run(() =>
                {
                    var memoryCacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                    .SetPriority(CacheItemPriority.Normal);

                    _memoryCache.Set(cacheKey, value, memoryCacheEntryOptions);
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task SetCache<T>(string cacheKey, T value, MemoryCacheEntryOptions memoryCacheEntryOptions)
        {
            try
            {
                await Task.Run(() =>
                {
                    _memoryCache.Set(cacheKey, value, memoryCacheEntryOptions);
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
