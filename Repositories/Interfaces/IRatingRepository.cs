using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface IRatingRepository
    {
        Task<Rating?> GetByIdAsync(Guid id);
        Task<IEnumerable<Rating>> GetByVenueIdAsync(Guid venueId);
        Task<IEnumerable<Rating>> GetByCourtIdAsync(Guid courtId);
        Task<IEnumerable<Rating>> GetByPlayerIdAsync(Guid playerId);
        Task<IEnumerable<Rating>> GetByGameIdAsync(Guid gameId);
        Task<Rating?> GetByUserAndGameAndTypeAsync(Guid userId, Guid gameId, RatingType type);
        Task<bool> HasRatedAsync(Guid userId, Guid gameId, RatingType type);
        Task AddAsync(Rating rating);
        Task UpdateAsync(Rating rating);
        Task SaveChangesAsync();
    }
}
