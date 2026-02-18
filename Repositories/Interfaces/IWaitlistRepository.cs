using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface IWaitlistRepository
    {
        Task<Waitlist?> GetByIdAsync(Guid id);
        Task<Waitlist?> GetByGameAndUserAsync(Guid gameId, Guid userId);
        Task<IEnumerable<Waitlist>> GetByGameIdAsync(Guid gameId);
        Task<IEnumerable<Waitlist>> GetByUserIdAsync(Guid userId);
        Task<bool> ExistsAsync(Guid gameId, Guid userId);
        Task<int> GetCountByGameIdAsync(Guid gameId);
        Task AddAsync(Waitlist waitlist);
        Task RemoveAsync(Waitlist waitlist);
        Task SaveChangesAsync();
    }
}
