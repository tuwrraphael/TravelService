using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OAuthApiClient
{
    public class MemoryCacheTokenStore : ITokenStore
    {
        private readonly IMemoryCache memoryCache;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public MemoryCacheTokenStore(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public async Task<ITokenAccessor> Get(string name)
        {
            var semaphore = semaphores.GetOrAdd(name, a => new SemaphoreSlim(1));
            await semaphore.WaitAsync();
            return new MemoryCacheTokenAccessor(memoryCache, semaphore, name);
        }

        private class MemoryCacheTokenAccessor : ITokenAccessor
        {
            private bool disposed = false;
            private readonly IMemoryCache memoryCache;
            private readonly SemaphoreSlim semaphore;
            private readonly string name;

            public MemoryCacheTokenAccessor(IMemoryCache memoryCache, SemaphoreSlim semaphore, string name)
            {
                this.memoryCache = memoryCache;
                this.semaphore = semaphore;
                this.name = name;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    semaphore.Release();
                }
            }

            public StoredToken Get()
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(nameof(MemoryCacheTokenAccessor));
                }
                var tokens = memoryCache.Get<StoredToken>(name);
                return tokens ?? new StoredToken() { AccessToken = null, AccesTokenExpires = null };
            }

            public async Task Update(StoredToken tokens)
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(nameof(MemoryCacheTokenAccessor));
                }
                memoryCache.Set(name, tokens);
            }
        }
    }
}
