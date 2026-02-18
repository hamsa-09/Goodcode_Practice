using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class DistributedLockService : IDistributedLockService
    {
        private readonly IMemoryCache _cache;
        private const string LockKeyPrefix = "slot_lock:";

        // In-memory lock synchronization object to prevent race conditions during the check-then-set calculation
        private static readonly object _syncLock = new object();

        public DistributedLockService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockDuration)
        {
            var fullKey = $"{LockKeyPrefix}{lockKey}";

            lock (_syncLock)
            {
                if (_cache.TryGetValue(fullKey, out _))
                {
                    return Task.FromResult(false);
                }

                // Create cache entry options
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(lockDuration);

                // Set the lock
                _cache.Set(fullKey, "LOCKED", cacheEntryOptions);
            }

            return Task.FromResult(true);
        }

        public Task ReleaseLockAsync(string lockKey)
        {
            var fullKey = $"{LockKeyPrefix}{lockKey}";
            _cache.Remove(fullKey);
            return Task.CompletedTask;
        }

        public Task<bool> IsLockedAsync(string lockKey)
        {
            var fullKey = $"{LockKeyPrefix}{lockKey}";
            var isLocked = _cache.TryGetValue(fullKey, out _);
            return Task.FromResult(isLocked);
        }
    }
}
