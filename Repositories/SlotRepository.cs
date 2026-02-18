using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;

namespace Assignment_Example_HU.Repositories
{
    public class SlotRepository : ISlotRepository
    {
        private readonly AppDbContext _dbContext;

        public SlotRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Slot?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Slots
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Slot?> GetByIdWithCourtAsync(Guid id)
        {
            return await _dbContext.Slots
                .Include(s => s.Court)
                    .ThenInclude(c => c.Venue)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Slots.AnyAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Slot>> GetAvailableSlotsAsync(
            Guid? courtId = null,
            Guid? venueId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbContext.Slots
                .Include(s => s.Court)
                    .ThenInclude(c => c.Venue)
                .AsQueryable();

            // Filter by court
            if (courtId.HasValue)
            {
                query = query.Where(s => s.CourtId == courtId.Value);
            }

            // Filter by venue
            if (venueId.HasValue)
            {
                query = query.Where(s => s.Court.VenueId == venueId.Value);
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                query = query.Where(s => s.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.StartTime <= endDate.Value);
            }

            // Only show available or locked slots (not booked, cancelled, expired, completed)
            query = query.Where(s => s.Status == SlotStatus.Available || s.Status == SlotStatus.Locked);

            // Don't show past slots
            query = query.Where(s => s.StartTime >= DateTime.UtcNow);

            return await query
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Slot>> GetExpiredLocksAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Slots
                .Where(s => s.Status == SlotStatus.Locked &&
                           s.LockedUntil.HasValue &&
                           s.LockedUntil.Value < now)
                .ToListAsync();
        }

        public async Task AddAsync(Slot slot)
        {
            await _dbContext.Slots.AddAsync(slot);
        }

        public async Task UpdateAsync(Slot slot)
        {
            _dbContext.Slots.Update(slot);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
