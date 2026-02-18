using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface IDiscountRepository
    {
        Task<Discount?> GetByIdAsync(Guid id);
        Task<IEnumerable<Discount>> GetByVenueIdAsync(Guid venueId);
        Task<IEnumerable<Discount>> GetByCourtIdAsync(Guid courtId);
        Task<IEnumerable<Discount>> GetActiveDiscountsAsync();
        Task AddAsync(Discount discount);
        Task UpdateAsync(Discount discount);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}
