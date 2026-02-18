using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface ISlotRepository
    {
        Task<Slot?> GetByIdAsync(Guid id);
        Task<Slot?> GetByIdWithCourtAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<Slot>> GetAvailableSlotsAsync(
            Guid? courtId = null,
            Guid? venueId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<IEnumerable<Slot>> GetExpiredLocksAsync();
        Task AddAsync(Slot slot);
        Task UpdateAsync(Slot slot);
        Task SaveChangesAsync();
    }
}
