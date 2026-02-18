using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface IGameService
    {
        Task<GameDto> CreateGameAsync(CreateGameDto dto, Guid userId);
        Task<GameDto?> GetGameByIdAsync(Guid id);
        Task<IEnumerable<GameDto>> GetGamesByUserIdAsync(Guid userId);
        Task<GameDto> JoinGameAsync(JoinGameDto dto, Guid userId);
        Task<bool> LeaveGameAsync(LeaveGameDto dto, Guid userId);
        Task<bool> CancelGameAsync(Guid gameId, Guid userId);
        Task<IEnumerable<Game>> GetPendingGamesWithLowPlayersAsync(int minPlayers);
        Task CancelGamesWithLowPlayersAsync();
    }
}
