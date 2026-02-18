using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface ICourtRepository
    {
        Task<Court?> GetByIdAsync(Guid id);
        Task<Court?> GetByIdWithVenueAsync(Guid id);
        Task<IEnumerable<Court>> GetByVenueIdAsync(Guid venueId);
        Task<IEnumerable<Court>> GetAllAsync();
        Task AddAsync(Court court);
        Task UpdateAsync(Court court);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}
