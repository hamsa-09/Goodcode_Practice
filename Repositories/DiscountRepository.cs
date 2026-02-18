using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;

namespace Assignment_Example_HU.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly AppDbContext _dbContext;

        public DiscountRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Discount?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Discounts
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Discount>> GetByVenueIdAsync(Guid venueId)
        {
            return await _dbContext.Discounts
                .AsNoTracking()
                .Where(d => d.VenueId == venueId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Discount>> GetByCourtIdAsync(Guid courtId)
        {
            return await _dbContext.Discounts
                .AsNoTracking()
                .Where(d => d.CourtId == courtId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Discount>> GetActiveDiscountsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.Discounts
                .AsNoTracking()
                .Where(d => d.IsActive && d.ValidFrom <= now && d.ValidTo >= now)
                .ToListAsync();
        }

        public async Task AddAsync(Discount discount)
        {
            await _dbContext.Discounts.AddAsync(discount);
        }

        public async Task UpdateAsync(Discount discount)
        {
            _dbContext.Discounts.Update(discount);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Discounts.AnyAsync(d => d.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
