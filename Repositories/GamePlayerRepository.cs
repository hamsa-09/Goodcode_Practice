using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;

namespace Assignment_Example_HU.Repositories
{
    public class GamePlayerRepository : IGamePlayerRepository
    {
        private readonly AppDbContext _dbContext;

        public GamePlayerRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(GamePlayer gamePlayer)
        {
            await _dbContext.GamePlayers.AddAsync(gamePlayer);
        }

        public async Task RemoveAsync(GamePlayer gamePlayer)
        {
            _dbContext.GamePlayers.Remove(gamePlayer);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
