using System;
using System.Threading.Tasks;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IDistributedLockService
    {
        Task<bool> AcquireLockAsync(string lockKey, TimeSpan lockDuration);
        Task ReleaseLockAsync(string lockKey);
        Task<bool> IsLockedAsync(string lockKey);
    }
}
