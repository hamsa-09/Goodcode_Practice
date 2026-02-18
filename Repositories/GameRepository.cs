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
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _dbContext;

        public GameRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Game?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Games
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Game?> GetByIdWithPlayersAsync(Guid id)
        {
            return await _dbContext.Games
                .Include(g => g.Players)
                    .ThenInclude(gp => gp.User)
                .Include(g => g.CreatedByUser)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Game>> GetByStatusAsync(GameStatus status)
        {
            return await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetByCreatedByUserIdAsync(Guid userId)
        {
            return await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.CreatedByUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetPendingGamesWithLowPlayersAsync(int minPlayers)
        {
            return await _dbContext.Games
                .Include(g => g.Players)
                .Where(g => g.Status == GameStatus.Pending)
                .ToListAsync()
                .ContinueWith(task =>
                {
                    return task.Result.Where(g => g.Players.Count < g.MinPlayers);
                });
        }

        public async Task AddAsync(Game game)
        {
            await _dbContext.Games.AddAsync(game);
        }

        public async Task UpdateAsync(Game game)
        {
            _dbContext.Games.Update(game);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Games.AnyAsync(g => g.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
