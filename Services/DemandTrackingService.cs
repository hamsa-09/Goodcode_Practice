using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class DemandTrackingService : IDemandTrackingService
    {
        private readonly IDistributedCache _cache;
        private const string ViewerCountKeyPrefix = "slot_viewers:";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

        public DemandTrackingService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task IncrementViewerCountAsync(Guid slotId)
        {
            var key = GetCacheKey(slotId);
            var currentCount = await GetViewerCountAsync(slotId);
            currentCount++;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            };

            await _cache.SetStringAsync(key, currentCount.ToString(), options);
        }

        public async Task<int> GetViewerCountAsync(Guid slotId)
        {
            var key = GetCacheKey(slotId);
            var cachedValue = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedValue))
            {
                return 0;
            }

            return int.TryParse(cachedValue, out var count) ? count : 0;
        }

        public async Task ResetViewerCountAsync(Guid slotId)
        {
            var key = GetCacheKey(slotId);
            await _cache.RemoveAsync(key);
        }

        private static string GetCacheKey(Guid slotId)
        {
            return $"{ViewerCountKeyPrefix}{slotId}";
        }
    }
}
