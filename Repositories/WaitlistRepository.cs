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
    public class WaitlistRepository : IWaitlistRepository
    {
        private readonly AppDbContext _dbContext;

        public WaitlistRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Waitlist?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Waitlists
                .Include(w => w.User)
                .Include(w => w.Game)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Waitlist?> GetByGameAndUserAsync(Guid gameId, Guid userId)
        {
            return await _dbContext.Waitlists
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.GameId == gameId && w.UserId == userId);
        }

        public async Task<IEnumerable<Waitlist>> GetByGameIdAsync(Guid gameId)
        {
            return await _dbContext.Waitlists
                .Include(w => w.User)
                .Where(w => w.GameId == gameId)
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.JoinedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Waitlist>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Waitlists
                .Include(w => w.Game)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.JoinedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid gameId, Guid userId)
        {
            return await _dbContext.Waitlists
                .AnyAsync(w => w.GameId == gameId && w.UserId == userId);
        }

        public async Task<int> GetCountByGameIdAsync(Guid gameId)
        {
            return await _dbContext.Waitlists
                .CountAsync(w => w.GameId == gameId);
        }

        public async Task AddAsync(Waitlist waitlist)
        {
            await _dbContext.Waitlists.AddAsync(waitlist);
        }

        public async Task RemoveAsync(Waitlist waitlist)
        {
            _dbContext.Waitlists.Remove(waitlist);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
