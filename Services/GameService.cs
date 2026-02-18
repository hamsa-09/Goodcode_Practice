using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly UserManager<User> _userManager;
        private readonly IGamePlayerRepository _gamePlayerRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IMapper _mapper;

        public GameService(
            IGameRepository gameRepository,
            UserManager<User> userManager,
            IGamePlayerRepository gamePlayerRepository,
            ISlotRepository slotRepository,
            IMapper mapper)
        {
            _gameRepository = gameRepository;
            _userManager = userManager;
            _gamePlayerRepository = gamePlayerRepository;
            _slotRepository = slotRepository;
            _mapper = mapper;
        }

        public async Task<GameDto> CreateGameAsync(CreateGameDto dto, Guid userId)
        {
            var game = _mapper.Map<Game>(dto);
            game.Id = Guid.NewGuid();
            game.Status = GameStatus.Pending;
            game.CreatedByUserId = userId;

            // Creator automatically joins the game
            var creatorPlayer = new GamePlayer
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                UserId = userId
            };

            await _gameRepository.AddAsync(game);
            await _gamePlayerRepository.AddAsync(creatorPlayer);
            await _gameRepository.SaveChangesAsync();

            var gameWithPlayers = await _gameRepository.GetByIdWithPlayersAsync(game.Id);
            return _mapper.Map<GameDto>(gameWithPlayers!);
        }

        public async Task<GameDto?> GetGameByIdAsync(Guid id)
        {
            var game = await _gameRepository.GetByIdWithPlayersAsync(id);
            if (game == null) return null;

            return _mapper.Map<GameDto>(game);
        }

        public async Task<IEnumerable<GameDto>> GetGamesByUserIdAsync(Guid userId)
        {
            var games = await _gameRepository.GetByCreatedByUserIdAsync(userId);
            var gameDtos = new List<GameDto>();

            foreach (var game in games)
            {
                var gameWithPlayers = await _gameRepository.GetByIdWithPlayersAsync(game.Id);
                if (gameWithPlayers != null)
                {
                    gameDtos.Add(_mapper.Map<GameDto>(gameWithPlayers));
                }
            }

            return gameDtos;
        }

        public async Task<GameDto> JoinGameAsync(JoinGameDto dto, Guid userId)
        {
            var game = await _gameRepository.GetByIdWithPlayersAsync(dto.GameId);
            if (game == null)
            {
                throw new InvalidOperationException("Game not found.");
            }

            if (game.Status != GameStatus.Pending)
            {
                throw new InvalidOperationException("Game is not open for joining.");
            }

            if (game.Players.Any(p => p.UserId == userId))
            {
                throw new InvalidOperationException("You are already in this game.");
            }

            if (game.Players.Count >= game.MaxPlayers)
            {
                throw new InvalidOperationException("Game is full.");
            }

            var player = new GamePlayer
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                UserId = userId
            };

            await _gamePlayerRepository.AddAsync(player);
            await _gamePlayerRepository.SaveChangesAsync();

            var updatedGame = await _gameRepository.GetByIdWithPlayersAsync(dto.GameId);
            return _mapper.Map<GameDto>(updatedGame!);
        }

        public async Task<bool> LeaveGameAsync(LeaveGameDto dto, Guid userId)
        {
            var game = await _gameRepository.GetByIdWithPlayersAsync(dto.GameId);
            if (game == null)
            {
                throw new InvalidOperationException("Game not found.");
            }

            if (game.Status != GameStatus.Pending)
            {
                throw new InvalidOperationException("Cannot leave a game that is not pending.");
            }

            var player = game.Players.FirstOrDefault(p => p.UserId == userId);
            if (player == null)
            {
                throw new InvalidOperationException("You are not in this game.");
            }

            if (game.CreatedByUserId == userId && game.Players.Count > 1)
            {
                throw new InvalidOperationException("Game creator cannot leave if other players have joined.");
            }

            await _gamePlayerRepository.RemoveAsync(player);
            await _gamePlayerRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelGameAsync(Guid gameId, Guid userId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                throw new InvalidOperationException("Game not found.");
            }

            if (game.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException("Only the game creator can cancel the game.");
            }

            game.Status = GameStatus.Cancelled;
            game.CancelledAt = DateTime.UtcNow;

            await _gameRepository.UpdateAsync(game);
            await _gameRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Game>> GetPendingGamesWithLowPlayersAsync(int minPlayers)
        {
            return await _gameRepository.GetPendingGamesWithLowPlayersAsync(minPlayers);
        }

        public async Task CancelGamesWithLowPlayersAsync()
        {
            var games = await _gameRepository.GetPendingGamesWithLowPlayersAsync(0);
            var now = DateTime.UtcNow;

            foreach (var game in games)
            {
                // Auto-cancel if game starts soon (e.g., within 1 hour) and doesn't have min players
                var gameWithPlayers = await _gameRepository.GetByIdWithPlayersAsync(game.Id);
                if (gameWithPlayers != null)
                {
                    var slot = await _slotRepository.GetByIdAsync(gameWithPlayers.SlotId);
                    if (slot != null && slot.StartTime <= now.AddHours(1))
                    {
                        if (gameWithPlayers.Players.Count < gameWithPlayers.MinPlayers)
                        {
                            gameWithPlayers.Status = GameStatus.Cancelled;
                            gameWithPlayers.CancelledAt = now;
                            await _gameRepository.UpdateAsync(gameWithPlayers);
                        }
                    }
                }
            }

            await _gameRepository.SaveChangesAsync();
        }

        public async Task CompleteGamesAsync()
        {
            var now = DateTime.UtcNow;
            var ongoingGames = await _gameRepository.GetAllAsync();
            var pendingOrBookedGames = ongoingGames.Where(g => g.Status == GameStatus.Pending || g.Status == GameStatus.Confirmed);

            foreach (var game in pendingOrBookedGames)
            {
                var slot = await _slotRepository.GetByIdAsync(game.SlotId);
                if (slot != null && slot.EndTime <= now)
                {
                    game.Status = GameStatus.Completed;
                    await _gameRepository.UpdateAsync(game);

                    var slotToUpdate = await _slotRepository.GetByIdWithCourtAsync(game.SlotId);
                    if (slotToUpdate != null && slotToUpdate.Status == SlotStatus.Booked)
                    {
                        slotToUpdate.Status = SlotStatus.Completed;
                        await _slotRepository.UpdateAsync(slotToUpdate);
                    }
                }
            }

            await _gameRepository.SaveChangesAsync();
            await _slotRepository.SaveChangesAsync();
        }
    }
}
