using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Models.Directions;
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

        public async Task<DirectionsResult> GetAsync(string key)
        {
            return memoryCache.Get<DirectionsResult>(key);
        }

        public async Task<DirectionsResult> PutAsync(TransitDirections directions)
        {
            var key = Guid.NewGuid().ToString();
            var res = new DirectionsResult()
            {
                CacheKey = key,
                TransitDirections = directions
            };
            memoryCache.Set(key, res);
            return res;
        }
    }
}