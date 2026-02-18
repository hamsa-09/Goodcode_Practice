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
    public class VenueRepository : IVenueRepository
    {
        private readonly AppDbContext _dbContext;

        public VenueRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Venue?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Venues
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Venue?> GetByIdWithOwnerAsync(Guid id)
        {
            return await _dbContext.Venues
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Venue>> GetByOwnerIdAsync(Guid ownerId)
        {
            return await _dbContext.Venues
                .AsNoTracking()
                .Where(v => v.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venue>> GetAllAsync()
        {
            return await _dbContext.Venues
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Venue venue)
        {
            await _dbContext.Venues.AddAsync(venue);
        }

        public async Task UpdateAsync(Venue venue)
        {
            _dbContext.Venues.Update(venue);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Venues.AnyAsync(v => v.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
