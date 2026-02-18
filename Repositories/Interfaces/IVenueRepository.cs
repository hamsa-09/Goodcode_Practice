using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface IVenueRepository
    {
        Task<Venue?> GetByIdAsync(Guid id);
        Task<Venue?> GetByIdWithOwnerAsync(Guid id);
        Task<IEnumerable<Venue>> GetByOwnerIdAsync(Guid ownerId);
        Task<IEnumerable<Venue>> GetAllAsync();
        Task AddAsync(Venue venue);
        Task UpdateAsync(Venue venue);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}
