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
    public class RatingRepository : IRatingRepository
    {
        private readonly AppDbContext _dbContext;

        public RatingRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Rating?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Ratings
                .Include(r => r.RatedBy)
                .Include(r => r.Venue)
                .Include(r => r.Court)
                .Include(r => r.Player)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Rating>> GetByVenueIdAsync(Guid venueId)
        {
            return await _dbContext.Ratings
                .Include(r => r.RatedBy)
                .Where(r => r.VenueId == venueId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rating>> GetByCourtIdAsync(Guid courtId)
        {
            return await _dbContext.Ratings
                .Include(r => r.RatedBy)
                .Where(r => r.CourtId == courtId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rating>> GetByPlayerIdAsync(Guid playerId)
        {
            return await _dbContext.Ratings
                .Include(r => r.RatedBy)
                .Where(r => r.PlayerId == playerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rating>> GetByGameIdAsync(Guid gameId)
        {
            return await _dbContext.Ratings
                .Include(r => r.RatedBy)
                .Where(r => r.GameId == gameId)
                .ToListAsync();
        }

        public async Task<Rating?> GetByUserAndGameAndTypeAsync(Guid userId, Guid gameId, RatingType type)
        {
            return await _dbContext.Ratings
                .FirstOrDefaultAsync(r => r.RatedById == userId && 
                                         r.GameId == gameId && 
                                         r.Type == type);
        }

        public async Task<bool> HasRatedAsync(Guid userId, Guid gameId, RatingType type)
        {
            return await _dbContext.Ratings
                .AnyAsync(r => r.RatedById == userId && 
                             r.GameId == gameId && 
                             r.Type == type);
        }

        public async Task AddAsync(Rating rating)
        {
            await _dbContext.Ratings.AddAsync(rating);
        }

        public async Task UpdateAsync(Rating rating)
        {
            _dbContext.Ratings.Update(rating);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
