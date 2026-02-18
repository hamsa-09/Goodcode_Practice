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
    public class CourtRepository : ICourtRepository
    {
        private readonly AppDbContext _dbContext;

        public CourtRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Court?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Courts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Court?> GetByIdWithVenueAsync(Guid id)
        {
            return await _dbContext.Courts
                .Include(c => c.Venue)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Court>> GetByVenueIdAsync(Guid venueId)
        {
            return await _dbContext.Courts
                .AsNoTracking()
                .Where(c => c.VenueId == venueId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Court>> GetAllAsync()
        {
            return await _dbContext.Courts
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Court court)
        {
            await _dbContext.Courts.AddAsync(court);
        }

        public async Task UpdateAsync(Court court)
        {
            _dbContext.Courts.Update(court);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Courts.AnyAsync(c => c.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
