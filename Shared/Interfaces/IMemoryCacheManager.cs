using Microsoft.Extensions.Caching.Memory;

namespace Shared.Interfaces
{
    public interface IMemoryCacheManager
    {
        Task SetCache<T>(string cacheKey, T value);
        Task SetCache<T>(string cacheKey, T value, MemoryCacheEntryOptions memoryCacheEntryOptions);
    }
}
