using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Impl
{
    public class DirectionsCache : IDirectionsCache
    {
        private readonly IMemoryCache memoryCache;

        public DirectionsCache(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public async Task<Plan> GetAsync(string key)
        {
            return memoryCache.Get<Plan>(key);
        }

        public async Task PutAsync(string id, Plan directions)
        {
            memoryCache.Set(id, directions);
        }
    }
}