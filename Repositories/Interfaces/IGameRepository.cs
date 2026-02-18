using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface IGameRepository
    {
        Task<Game?> GetByIdAsync(Guid id);
        Task<Game?> GetByIdWithPlayersAsync(Guid id);
        Task<IEnumerable<Game>> GetByStatusAsync(GameStatus status);
        Task<IEnumerable<Game>> GetByCreatedByUserIdAsync(Guid userId);
        Task<IEnumerable<Game>> GetPendingGamesWithLowPlayersAsync(int minPlayers);
        Task AddAsync(Game game);
        Task UpdateAsync(Game game);
        Task<bool> ExistsAsync(Guid id);
        Task SaveChangesAsync();
    }
}
